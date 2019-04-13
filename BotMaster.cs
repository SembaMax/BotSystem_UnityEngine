using UnityEngine;
using System.Collections;


public class BotMaster : MonoBehaviour
{

    public bool fightMode;
    public delegate void EnemyEventHandler();
    public event EnemyEventHandler EventEnemyDie;
    public event EnemyEventHandler EventEnemyRespawn;
    public event EnemyEventHandler EventEnemyReachNavTarget;

    public delegate void RestartNavEventHandler(float restartAfterTime);
    public event RestartNavEventHandler EventEnemyRestartNavTrip;

    public delegate IEnumerator EquipEventHandler();
    public event EquipEventHandler EventEnemySelectWeapon;

    public delegate void WalkEventHandler(Vector3 motionVector);
    public event WalkEventHandler EventEnemyWalk;

    public delegate void AttackEventHandler(bool isFiring);
    public event AttackEventHandler EventEnemyAttack;

    public delegate void HealthEventHandler(int health);
    public event HealthEventHandler EventEnemyHealthDeduction;

    public delegate void EnemyFoundTargetHandler(Transform target);
    public event EnemyFoundTargetHandler EventEnemyFoundTarget;
    public event EnemyFoundTargetHandler EventInjuredAllyFoundTarget;

    public delegate void EnemyLostTargetHandler(Transform lastSeenPlace);
    public event EnemyLostTargetHandler EventEnemyLostTarget;
    public event EnemyLostTargetHandler EventInjuredAllyRecovered;

    public bool isOnRoute;
    public bool isNavPaused;
    public float walkSpeed;
    public BotName.EnemyName enemyName;
    public bool isAttacking;
    public float struckRate = 5;
    public float nextStruck;
    public float getDamageRate = 1;
    public float nextGetDamage;


    public void CallEventEnemyHealthDeduction(int health)
    {
        if (EventEnemyHealthDeduction != null)
        {

            EventEnemyHealthDeduction(health);
        }
    }

    public void CallEventEnemyFoundTarget(Transform tagTransform)
    {
        
        if (EventEnemyFoundTarget != null)
        {
            EventEnemyFoundTarget(tagTransform);
        }
    }

    public void CallEventInjuredAllyFoundTarget(Transform tagTransform)
    {

        if (EventInjuredAllyFoundTarget != null)
        {
            EventInjuredAllyFoundTarget(tagTransform);
        }
    }

    public void CallEventInjuredAllyRecovered(Transform tagTransform)
    {
        if (EventInjuredAllyRecovered != null)
        {
            EventInjuredAllyRecovered(tagTransform);
        }
    }

    public void CallEventEnemyLostTarget(Transform tagTransform)
    {
        if (EventEnemyLostTarget != null)
        {
            EventEnemyLostTarget(tagTransform);
        }
    }

    public void CallEventEnemyDie()
    {
        if (EventEnemyDie != null)
        {
            EventEnemyDie();
        }
    }

    public void CallEventEnemyRespawn()
    {
        if (EventEnemyRespawn != null)
        {
            EventEnemyRespawn();
        }
    }

    public void CallEventEnemyWalk(Vector3 destenation)
    {
        isOnRoute = true;
        if (EventEnemyWalk != null)
        {
            EventEnemyWalk(destenation);
        }
    }

    public void CallEventEnemyReachNavTarget()
    {
        isOnRoute = false;
        if (EventEnemyReachNavTarget != null)
        {
            EventEnemyReachNavTarget();
        }
    }

    public void CallEventEnemyRestartNavTrip(float restartAfterTime)
    {
        if (EventEnemyRestartNavTrip != null)
        {
            EventEnemyRestartNavTrip(restartAfterTime);
        }
    }

    public void CallEventEnemyAttack(bool isFiring)
    {
        if (EventEnemyAttack != null)
        {
            EventEnemyAttack(isFiring);
        }
    }

    public void CallEventEnemySelectWeapon()
    {
        if (EventEnemySelectWeapon != null)
        {
            StartCoroutine(EventEnemySelectWeapon());
        }
    }

    void DisableThis()
    {
        this.enabled = false;
        DestroyImmediate(gameObject);
    }

    void OnEnable()
    {
        SetInitialReferences();
    }

    void OnDisable()
    {

    }

    void SetInitialReferences()
    {
        fightMode = false;
        isAttacking = false;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}