using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Networking;


public class BotNavPursue : NetworkBehaviour
{

    private BotMaster _botMaster;
    private BotDetection _botDetection;
    private NavMeshAgent _myNavMeshAgent;
    private float _checkRate;
    private float _nextCheck;

    void OnEnable()
    {
        SetInitialReferences();
        _botMaster.EventEnemyLostTarget += TryToChaseTarget;
    }

    void OnDisable()
    {
        _botMaster.EventEnemyLostTarget -= TryToChaseTarget;
    }

    void SetInitialReferences()
    {
        _botMaster = GetComponent<BotMaster>();
        _botDetection = GetComponent<BotDetection>();
        _checkRate = Random.Range(0.1f, 0.2f);
        if (GetComponent<NavMeshAgent>() != null)
        {
            _myNavMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    void Update()
    {

    }

    void TryToChaseTarget(Transform lastSeenPlace)
    {
        if (lastSeenPlace != null && _myNavMeshAgent != null && Time.time > _nextCheck && isServer) //&& !_botMaster.isNavPaused
        {
            _nextCheck = Time.time + _checkRate;
            _myNavMeshAgent.ResetPath();
            _myNavMeshAgent.stoppingDistance = 4f;
            _myNavMeshAgent.acceleration = 8f;
            _myNavMeshAgent.SetDestination(lastSeenPlace.position);
            _botMaster.CallEventEnemyWalk(lastSeenPlace.position);
        }
    }

    void DisableThis()
    {
        if (_myNavMeshAgent != null)
        {
            _myNavMeshAgent.enabled = false;
        }

        this.enabled = false;
    }

}
