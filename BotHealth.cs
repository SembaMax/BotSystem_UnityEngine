using UnityEngine;
using System.Collections;


public class BotHealth : MonoBehaviour
{
    private BotMaster enemyMaster;
    public int enemyHealth = 100;

    void OnEnable()
    {
        setInitialReferences();
        enemyMaster.EventEnemyHealthDeduction += DeductHealth;
    }

    void OnDisable()
    {
        enemyMaster.EventEnemyHealthDeduction -= DeductHealth;
    }

    void setInitialReferences()
    {
        enemyMaster = GetComponent<BotMaster>();
    }

    void DeductHealth(int damage, bool high)
    {
        enemyHealth -= damage;

        if (enemyHealth <= 0)
        {
            enemyHealth = 0;
            enemyMaster.CallEventEnemyDie();
        }
    }
}