using UnityEngine;
using System.Collections;

public class BotWeapon : MonoBehaviour
{

    private BotMaster botMaster;
    public LayerMask mask;
    public int damage = 15;

    void OnCollisionEnter(Collision collision)
    {

        if ((mask.value & 1 << collision.gameObject.layer) == 1 << collision.gameObject.layer && botMaster.isAttacking)
        {
            collision.gameObject.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver); 
        }
    }

    public void Blocked()
    {
        damage = 0;
    }

    public void UnBlocked()
    {
        damage = 15;
    }

    void OnEnable()
    {
        setInitialReferences();
    }

    void OnDisable()
    {

    }

    void setInitialReferences()
    {
        enemyMaster = transform.GetComponentInParent<BotMaster>();
    }

    void DisableThis()
    {
        damage = 0;
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
