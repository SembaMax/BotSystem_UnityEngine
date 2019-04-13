using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;
using System.Linq;

public class BotMedic : NetworkBehaviour {

    private BotMaster _botMaster;
    private NavMeshAgent _navMeshAgent;
    private BotNavPause _enemyNavPause;
    private Animator _animator;
    private float _nextAttack;
    private float _attackRate = 0.2f;
    private float _attackRange = 20; //Same as detection radius
    public bool _isFiring = false;
    private bool _shouldSwitchEnemy = false;
    private List<KeyValuePair<Transform, float>> _seenTargets = new List<KeyValuePair<Transform, float>>();
    public int _currentAllyIndex = 0;
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

    private void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updateRotation = false;
    }

    void OnEnable()
    {
        SetInitialReferences();
        _botMaster.EventInjuredAllyFoundTarget += FindAttackTarget;
        _botMaster.EventInjuredAllyRecovered += LoseAttackTarget;
        _botMaster.EventEnemyDie += ResetData;

    }

    void OnDisable()
    {
        _botMaster.EventInjuredAllyFoundTarget -= FindAttackTarget;
        _botMaster.EventInjuredAllyRecovered -= LoseAttackTarget;
        _botMaster.EventEnemyDie -= ResetData;
    }

    void ResetData()
    {
        _isFiring = false;
    }

    void SetInitialReferences()
    {
        _animator = GetComponent<Animator>();
        _botMaster = GetComponent<BotMaster>();
        _enemyNavPause = GetComponent<BotNavPause>();
    }

    [ServerCallback]
    private void Update()
    {
        TryToHeal();
    }

    void FindAttackTarget(Transform targetTransform)
    {
        if (_seenTargets.Count == 0)
        {
            _shouldSwitchEnemy = true;
        }

            if (!_seenTargets.Any(x => x.Key == targetTransform.root))
            {
                _seenTargets.Add(new KeyValuePair<Transform, float>(targetTransform.root, Time.time));
            }
    }

    void LoseAttackTarget(Transform targetTransform)
    {
        if (Gun != null)
        {
            Gun.transform.localRotation = Quaternion.identity;
        }
        if (_currentAllyIndex < _seenTargets.Count && _seenTargets[_currentAllyIndex].Key == targetTransform.root)
        {
            _shouldSwitchEnemy = true;
        }

        _seenTargets.RemoveAll(x => x.Key == targetTransform.root);   

        if (_seenTargets.Count == 0)
        {
            _isFiring = false;
            _botMaster.CallEventEnemyAttack(false);
        }
    }

    void TryToHeal()
    {
        Transform targetTransform = null;
        if (_seenTargets.Count > 0)
        {
            if (_shouldSwitchEnemy)
            {
                _shouldSwitchEnemy = false;
                _currentAllyIndex = Random.Range(0, _seenTargets.Count);

            }
            if (_currentAllyIndex < _seenTargets.Count)
            {
                
                _nextAttack = Time.time + _attackRate;
                if (Vector3.Distance(transform.position, _seenTargets.ElementAt(_currentAllyIndex).Key.position) <= _attackRange)
                {
                    _isFiring = true;
                    Vector3 lookAtVector = new Vector3(_seenTargets.ElementAt(_currentAllyIndex).Key.position.x, transform.position.y, _seenTargets.ElementAt(_currentAllyIndex).Key.position.z);
                    targetTransform = _seenTargets.ElementAt(_currentAllyIndex).Key;
                    _botMaster.CallEventEnemyAttack(true);
                }
                else if (_isFiring && _seenTargets.Count > 0)
                {
                    _botMaster.CallEventEnemyLostTarget(_seenTargets.ElementAt(_currentAllyIndex).Key);
                }
            }
        }

        if (targetTransform == null)
        {
            LookAtTarget(transform, null);
            AimOnTarget(null);
        }
        else
        {
            LookAtTarget(transform, targetTransform.position);
            AimOnTarget(targetTransform);
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
            targetPoint = new Vector3(theTarget.Value.x, transform.position.y, theTarget.Value.z);
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
        _seenTargets.RemoveAll(x => x.Key.GetComponent<Health>()._currentHealth == x.Key.GetComponent<Health>()._maxHealth);
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
            if (target == null)
            {
                LookAtTarget(Gun.transform, null);
            }
            else if (target.gameObject.layer == GameConstants.ObjectiveLayer)
            {
                LookAtTarget(Gun.transform, target.position);
            }
            else
            {
                LookAtTarget(Gun.transform, target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Chest).position);
            }
        }
    }
}
