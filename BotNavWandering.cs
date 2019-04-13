using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Networking;

public class BotNavWandering : NetworkBehaviour
{

    private ArrayList _patrolPoints;
    private Transform _currentTargetPatrol;
    public bool ByPassServerCheck = false;
    private BotMaster _botMaster;
    private NavMeshAgent _myNavMeshAgent;
    private float _checkRate;
    private float _nextCheck;
    private Transform _myTransform;
    private float _wanderRange = 10;
    private NavMeshHit _navHit;
    private bool _isCurrentTargetReached = true;
    private bool _isEnemySeen = false;

    void OnEnable()
    {
        SetInitialReferences();
        _botMaster.EventEnemyReachNavTarget += CurrentTargetReached;
        _botMaster.EventEnemyFoundTarget += ISeeEnemy;
        _botMaster.EventEnemyLostTarget += ILostEnemy;
    }

    void OnDisable()
    {
        _botMaster.EventEnemyReachNavTarget -= CurrentTargetReached;
        _botMaster.EventEnemyFoundTarget -= ISeeEnemy;
        _botMaster.EventEnemyLostTarget -= ILostEnemy;
    }

    void CurrentTargetReached()
    {
        _isCurrentTargetReached = true;
    }

    void ISeeEnemy(Transform targetTransform)
    {
        _isEnemySeen = true;
        _isCurrentTargetReached = true;
    }

    void ILostEnemy(Transform lastSeenPlace)
    {
        _isEnemySeen = false;
    }

    void SetInitialReferences()
    {
        _patrolPoints = new ArrayList();
        Transform botTargets = GameObject.Find(GameConstants.BotTargetsName).transform;
        foreach (Transform child in botTargets)
        {
            _patrolPoints.Add(child);
        }
        _currentTargetPatrol = (Transform)_patrolPoints[Random.Range(0, _patrolPoints.Count - 1)];
        _botMaster = GetComponent<BotMaster>();
        _checkRate = Random.Range(0.3f, 0.4f);
        if (GetComponent<NavMeshAgent>() != null)
        {
            _myNavMeshAgent = GetComponent<NavMeshAgent>();
        }
        _myTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer && !ByPassServerCheck)
        {
            return;
        }

        SearchInPatrols();
    } 

    void SearchInPatrols()
    {
        //Usage: Bot searches in predifined places
        //get a random portal not equal to the current one
        if (!_isEnemySeen && _isCurrentTargetReached && _nextCheck < Time.time)
        {
            _nextCheck = Time.time + _checkRate;

            int nextPatrolIndex = Random.Range(0, _patrolPoints.Count - 1);
            Transform nextPatrol = (Transform)_patrolPoints[nextPatrolIndex];

            if (nextPatrol != _currentTargetPatrol && !_botMaster.isOnRoute && !_botMaster.isNavPaused)
            {
                _isCurrentTargetReached = false;
                _currentTargetPatrol = nextPatrol;
                _myNavMeshAgent.SetDestination(nextPatrol.position);
                _botMaster.CallEventEnemyWalk(nextPatrol.position);
            }
            else
            {
                SearchInPatrols();
            }
        }
    }
    
    bool RandomWanderTarget(Vector3 centre, float range, out Vector3 result)
    {
        //Usage: Bot searches in random places
        Vector3 randomPoint = centre + Random.insideUnitSphere * _wanderRange;
        if (NavMesh.SamplePosition(randomPoint, out _navHit, 1.0f, NavMesh.AllAreas))
        {
            result = _navHit.position;
            return true;
        }
        else
        {
            result = centre;
            return false;
        }
    }

    void DisableThis()
    {
        this.enabled = false;
    }

}