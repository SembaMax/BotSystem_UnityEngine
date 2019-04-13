using UnityEngine;
using System.Collections;
using UnityEngine.AI;


public class BotNavPause : MonoBehaviour
{
    public bool _isPaused;
    private BotMaster _botMaster;
    private NavMeshAgent _myNavMeshAgent;
    private Animator _myAnimator;
    private bool _attackFirstTime = true;

    void OnEnable()
    {
        SetInitialReferences();

        _botMaster.EventEnemyHealthDeduction += PauseWhenStruck;
        _botMaster.EventEnemyAttack += PauseWhileAttack;
        _botMaster.EventEnemyFoundTarget += PauseWhenFind;
        _botMaster.EventEnemyWalk += Walking;
        _botMaster.EventEnemyLostTarget += ContinueWhenLose;
        _botMaster.EventEnemyRestartNavTrip += RestartNavTrip;
        //TODO i think it should call restart nav on start detection from the begining... Let's see.
    }

    void OnDisable()
    {

        _botMaster.EventEnemyHealthDeduction -= PauseWhenStruck;
        _botMaster.EventEnemyAttack -= PauseWhileAttack;
        _botMaster.EventEnemyWalk -= Walking;
        _botMaster.EventEnemyFoundTarget -= PauseWhenFind;
        _botMaster.EventEnemyLostTarget -= ContinueWhenLose;
        _botMaster.EventEnemyRestartNavTrip -= RestartNavTrip;
    }

    void SetInitialReferences()
    {
        _botMaster = GetComponent<BotMaster>();
        _myAnimator = GetComponent<Animator>();
        if (GetComponent<NavMeshAgent>() != null)
        {
            _myNavMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    private void RestartNavTrip(float restartAfterTime)
    {
        ContinueNavigation();
    }

    private void PauseWhenStruck(int dummy, bool high)
    {
        if (_myNavMeshAgent != null && _myNavMeshAgent.enabled)
        {
            float stopAfterDelay = Random.Range(0.3f, 1f);
            Invoke("StopNavigation", stopAfterDelay);
        }
    }

    private void Walking(Vector3 dummy)
    {
        _attackFirstTime = true;
    }

    private void PauseWhileAttack(bool isFiring)
    {
        if (_myNavMeshAgent != null && _myNavMeshAgent.enabled && isFiring)
        {
            StopNavigation();
        }
        _attackFirstTime = false;
    }

    private void PauseWhenFind(Transform target)
    {
        if (_myNavMeshAgent != null && _myNavMeshAgent.enabled)
        {
            StopNavigation();
        }
    }


    private void PauseWhenReach()
    {
        if (_myNavMeshAgent != null && _myNavMeshAgent.enabled)
        {
            StopNavigation();
            StartCoroutine(RestartNavMeshAgent(pauseDuration));
        }
    }

    public void StopNavigation()
    {
        Utils.LogError("Pause");
        _myNavMeshAgent.isStopped = true;
        _botMaster.isNavPaused = true;
        _isPaused = true;
    }

    public void ContinueNavigation()
    {
        _myNavMeshAgent.isStopped = false;
        _myNavMeshAgent.ResetPath();
        _botMaster.isNavPaused = false;
        _isPaused = false;
    }

    private void ContinueWhenLose(Transform lastSeenPlace)
    {
        _attackFirstTime = true;
        if (_myNavMeshAgent != null && _myNavMeshAgent.enabled)
        {
            if(_isPaused)
            {
                ContinueNavigation();
            }
        }
    }

    void DisableThis()
    {
        StopAllCoroutines();
        this.enabled = false;
    }
}
