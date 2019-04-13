using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Networking;


public class BotNavDestinationReched : NetworkBehaviour
{
    private BotMaster _botMaster;
    private NavMeshAgent _myNavMeshAgent;
    private BotNavPause _botNavPause;
    private float _checkRate;
    private float _nextCheck;
    private bool _checkReach = true;

    void OnEnable()
    {
        SetInitialReferences();
        _botMaster.EventEnemyFoundTarget += DisableReachCheck;
        _botMaster.EventEnemyLostTarget += EnableReachCheck;
    }

    void OnDisable()
    {
        _botMaster.EventEnemyFoundTarget -= DisableReachCheck;
        _botMaster.EventEnemyLostTarget -= EnableReachCheck;
    }

    void EnableReachCheck(Transform lastSeen)
    {
        _checkReach = true;
    }

    void DisableReachCheck(Transform target)
    {
        _checkReach = false;
    }

    void SetInitialReferences()
    {
        _botMaster = GetComponent<BotMaster>();
        _botNavPause = GetComponent<BotNavPause>();
        _checkRate = Random.Range(0.2f, 0.3f);
        if (GetComponent<NavMeshAgent>() != null)
        {
            _myNavMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > _nextCheck)
        {
            _nextCheck = Time.time + _checkRate;
            CheckDestinationReached();
        }
    }

    void CheckDestinationReached()
    {
        if (_botMaster.isOnRoute)
        {
            if (_myNavMeshAgent.remainingDistance <= _myNavMeshAgent.stoppingDistance)
            {
                _botMaster.CallEventEnemyReachNavTarget();
            }
        }
    }

    void DisableThis()
    {
        this.enabled = false;
    }

}
