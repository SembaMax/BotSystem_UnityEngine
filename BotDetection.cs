using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.AI;

public class BotDetection : NetworkBehaviour
{
    [SerializeField] private Transform _head;
    [SerializeField] public LayerMask _enemyLayer;
    [SerializeField] public LayerMask _allyLayer;
    [SerializeField] public LayerMask _sightLayer;
    [SerializeField] public LayerMask _ignoreLayer;
    [SerializeField] private int _retrysToSee = 3;
    [SerializeField] private int HEALING_THRESHOLD = 25;
    private int _retrysCounter = 0;
    private Transform _lastSeenPlace;
    private Transform _lastSeenPlaceForAlly;
    private BotMaster _botMaster;
    private BotMedic _botMedic;
    private Transform _myTransform;
    public bool _isMedicBot = false;
    private float _checkRate;
    private float _nextCheck;
    private float _detectRadius = 20;
    private RaycastHit _hit;
    public bool _isEnemySeen = false;

    void OnEnable()
    {
        SetInitialReferences();
    }

    void OnDisable()
    {

    }

    void SetInitialReferences()
    {
        _botMaster = GetComponent<BotMaster>();
        _botMedic = GetComponent<BotMedic>();
        _myTransform = transform;

        if (_head == null)
        {
            _head = _myTransform;
        }

        _checkRate = Random.Range(0.8f, 1.2f);
    }

    // Use this for initialization
    void Start()
    {

    }

    void Update()
    {
        CarryOutEnemiesDetection();
    }

    void CarryOutEnemiesDetection()
    {
        if (Time.time > _nextCheck)
        {
            _nextCheck = Time.time + _checkRate;

            Collider[] colliders = Physics.OverlapSphere(_myTransform.position, _detectRadius, _sightLayer);

            if (colliders.Length > 0)
            {
                bool targetExist = false;
                foreach (Collider potentialTargetCollider in colliders) //Breaking the for loop because if we got the near one no need to attack the far one right now.
                {
                    if (CanPotentialTargetBeSeen(potentialTargetCollider.transform))
                    {
                        targetExist = true;
                        _retrysCounter = 0;
                        break;
                    }
                    else
                    {
                        _retrysCounter++;
                    }
                }
                if (_isEnemySeen && !targetExist && _retrysCounter > _retrysToSee)
                {
                    _isEnemySeen = false;
                    _botMaster.CallEventEnemyLostTarget(_lastSeenPlace);
                }
            }
            else
            {
                if (_isEnemySeen)
                {
                    _isEnemySeen = false;
                    _botMaster.CallEventEnemyLostTarget(_lastSeenPlace);
                }
                else if(_botMaster.isNavPaused)
                {
                    _botMaster.CallEventEnemyRestartNavTrip(0.05f);
                }
            }
        }
    }


    bool CanPotentialTargetBeSeen(Transform potentialTarget)
    {
        if (Physics.Linecast(_head.position, potentialTarget.position, out _hit))
        {
            if (_hit.transform == potentialTarget && _enemyLayer == (_enemyLayer | (1 << _hit.transform.gameObject.layer)))
            {
                _lastSeenPlace = potentialTarget;
                _botMaster.CallEventEnemyFoundTarget(potentialTarget); //Don't set any conditions on Found. it should keep calling found at every frame to update the LastSeenPlace.
                _isEnemySeen = true;
                return true;
            }
            else if (_isMedicBot && CheckIfAllyNeedsHealing(potentialTarget))
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        return false;
    }


    bool CheckIfAllyNeedsHealing(Transform potentialTarget)
    {
        if (potentialTarget.root.GetComponent<Health>())
        {
            var max = potentialTarget.root.GetComponent<Health>()._maxHealth;
            var current = potentialTarget.root.GetComponent<Health>()._currentHealth;
            bool doesNeedHealing = max - current > HEALING_THRESHOLD && max - current != max;
            if (_hit.transform == potentialTarget && potentialTarget.root != transform.root && _allyLayer == (_allyLayer | (1 << _hit.transform.gameObject.layer)) && doesNeedHealing)
            {
                _lastSeenPlaceForAlly = potentialTarget;
                _botMaster.CallEventInjuredAllyFoundTarget(potentialTarget); //Don't set any conditions on Found. it should keep calling found at every frame to update the LastSeenPlace.
                _isEnemySeen = false;
                return true;
            }
        }

        return false;
    }
}
