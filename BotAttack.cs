using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.AI;

public class BotAttack : NetworkBehaviour
{
    private BotMaster _botMaster;
    private BotMedic _botMedic;
    private Transform _myTransform;
    private Animator _animator;
    private BotNavPause _enemyNavPause;
    private NavMeshAgent _navMeshAgent;
    private float _attackRate = 0.2f;
    private float _switchEnemyRate = 10;
    private float _nextAttack;
    private float _nextSwitchEnemy;
    private float _attackRange = 20; //Same as detection radius
    public bool _isFiring = false;
    private Vector3 _lastPosition;
    private Gun _gun;
    private Gun Gun
    {
        get
        {
            if (_gun == null)
            {
                _gun = GetComponentInChildren<Gun>();
            }
            return _gun;
        }
    }

    private List<KeyValuePair<Transform, float>> _seenTargets = new List<KeyValuePair<Transform, float>>();
    public int _currentEnemyIndex = 0;

    private void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        SetInitialReferences();
        _botMaster.EventEnemyFoundTarget += FindAttackTarget;
        _botMaster.EventEnemyLostTarget += LoseAttackTarget;

    }


    void OnDisable()
    {
        _botMaster.EventEnemyFoundTarget -= FindAttackTarget;
        _botMaster.EventEnemyLostTarget -= LoseAttackTarget;

    }


    void SetInitialReferences()
    {
        _animator = GetComponent<Animator>();
        _botMaster = GetComponent<BotMaster>();
        _enemyNavPause = GetComponent<BotNavPause>();
        _botMedic = GetComponent<BotMedic>();
        _myTransform = transform;
    }

    //[ServerCallback]
    private void Update()
    {
        if (_botMedic._isFiring == false)
        {
            TryToAttack();
        }
    }

    void FindAttackTarget(Transform targetTransform)
    {
        if (_seenTargets.Count == 0)
        {
            _nextSwitchEnemy = Time.time;
        }
        if (targetTransform.gameObject.layer == GameConstants.ObjectiveLayer)
        {
            if (!_seenTargets.Any(x => x.Key == targetTransform))
            {
                _seenTargets.Add(new KeyValuePair<Transform, float>(targetTransform, Time.time));
            }
        }
        else
        {
            if (!_seenTargets.Any(x => x.Key == targetTransform.root) && targetTransform.root.GetComponent<NavMeshAgent>().enabled == true)
            {
                _seenTargets.Add(new KeyValuePair<Transform, float>(targetTransform.root, Time.time));
            }
        }
    }


    void LoseAttackTarget(Transform targetTransform)
    {
        if (Gun != null)
        {

            Gun.transform.localRotation = Quaternion.identity;
        }
        if (targetTransform.gameObject.layer == GameConstants.ObjectiveLayer) // Bot is attacking the objective of the map.
        {
            if (_currentEnemyIndex < _seenTargets.Count && _seenTargets[_currentEnemyIndex].Key == targetTransform)
            {
                _nextSwitchEnemy = Time.time;
            }
            _seenTargets.RemoveAll(x => x.Key == targetTransform);
        }
        else // Bot is attacking the a player. "targetTransform" may be a collider inside the ragdoll, so you need to get the root player object.
        {
            if (_currentEnemyIndex < _seenTargets.Count && _seenTargets[_currentEnemyIndex].Key == targetTransform.root)
            {
                _nextSwitchEnemy = Time.time;
            }
            _seenTargets.RemoveAll(x => x.Key == targetTransform.root);
            
        }

        if (_seenTargets.Count == 0)
        {
            _isFiring = false;
            _botMaster.CallEventEnemyAttack(false);
        }
    }


    void TryToAttack()
    {
        Transform targetTransform = null;
        if (_seenTargets.Count > 0)
        {
            if (Time.time > _nextSwitchEnemy)
            {
                _nextSwitchEnemy = Time.time + _switchEnemyRate;
                _currentEnemyIndex = Random.Range(0, _seenTargets.Count);

            }
            if (_currentEnemyIndex < _seenTargets.Count)
            {
                if (_seenTargets.Count > 1 && _seenTargets.ElementAt(_currentEnemyIndex).Key.gameObject.layer == GameConstants.ObjectiveLayer)
                {
                    if (_seenTargets.Count == 2)
                    {
                        _currentEnemyIndex = 1 - _currentEnemyIndex; //This trick to get the other element.
                    }
                    else
                    {
                        _currentEnemyIndex = Random.Range(0, _seenTargets.Count); //It will give a second chance for randomization behaviour.
                    }
                }
                _nextAttack = Time.time + _attackRate;
                if (Vector3.Distance(_myTransform.position, _seenTargets.ElementAt(_currentEnemyIndex).Key.position) <= _attackRange)
                {
                    _isFiring = true;
                    Vector3 lookAtVector = new Vector3(_seenTargets.ElementAt(_currentEnemyIndex).Key.position.x, _myTransform.position.y, _seenTargets.ElementAt(_currentEnemyIndex).Key.position.z);
                     _myTransform.LookAt(lookAtVector);
                    targetTransform = _seenTargets.ElementAt(_currentEnemyIndex).Key;
                    LookAtTarget(transform, targetTransform.position);
                    AimOnTarget(targetTransform);
                    if (_botMedic.enabled)
                    {
                        Utils.LogError("Looking At   " + targetTransform.name);
                    }
                    _botMaster.CallEventEnemyAttack(true);
                }
                else if (_isFiring && _seenTargets.Count > 0)
                {
                    _botMaster.CallEventEnemyLostTarget(_seenTargets.ElementAt(_currentEnemyIndex).Key);                        
                }
            }
        }
 
    }

    private void LookAtTarget(Transform theTransform, Vector3? theTarget)
    {
        Vector3 targetPoint = Vector3.zero;
        if (theTarget == null) // no target
        {
            var targetPosition = _navMeshAgent.pathEndPosition;
            targetPoint = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        }
        else
        {
            targetPoint = new Vector3(theTarget.Value.x, _myTransform.position.y, theTarget.Value.z);
        }

        var _direction = (targetPoint - transform.position).normalized;

        if (_direction != Vector3.zero)
        {
            var _lookRotation = Quaternion.LookRotation(_direction);
            theTransform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, GameConstants.BotRotationFactor * Time.deltaTime);
        }
    }

    [ServerCallback]
    private void LateUpdate()
    {
        _seenTargets.RemoveAll(x => Time.time - x.Value > 20);
        _seenTargets.RemoveAll(x => x.Key.GetComponent<NavMeshAgent>() ? x.Key.GetComponent<NavMeshAgent>().enabled == false : false); //Died bodies.
        if (_seenTargets.Count == 0 && _isFiring)
        {
            _isFiring = false;
            _botMaster.CallEventEnemyAttack(false);
        }
    }


    private void AimOnTarget(Transform target)
    {
        if (Gun)
        {
            if (target  == null)
            {
                LookAtTarget(Gun.transform, null);
            }
            else if (target.gameObject.layer == GameConstants.ObjectiveLayer)
            {
                //Gun.transform.LookAt(target);
                LookAtTarget(Gun.transform, target.position);
            }
            else
            {
                //Gun.transform.LookAt(target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Chest));
                LookAtTarget(Gun.transform, target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Chest).position);
            }
        }
    }

    void DisableThis()
    {
        _botMaster.CallEventEnemyAttack(false);
        this.enabled = false;
    }

}
