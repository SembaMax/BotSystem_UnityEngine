using UnityEngine;
using System.Collections;

public class BotTakeDamage : MonoBehaviour
{

    private BotMaster enemyMaster;
    private Rigidbody rigidBody;
    public int damageMultiplier = 1;
    public bool canTackDamage;
    public GameObject bloodPrefab;
    public GameObject bloodParticle;

    void OnEnable()
    {
        SetInitialReferences();
    }

    void OnDisable()
    {

    }

    void SetInitialReferences()
    {
        enemyMaster = transform.GetComponentInParent<BotMaster>();
        rigidBody = GetComponent<Rigidbody>();
        bloodPrefab = (GameObject)Resources.Load("Prefabs/Blood Particle", typeof(GameObject));
        canTakeDamage = true;
    }

    void OnCollisionEnter(Collision col)
    {
        if (canTakeDamage && col.collider.gameObject.tag == "PlayerWeapon" && this.enabled)
        {
            var weaponScript = col.collider.getComponent<WeaponScript>();
            ContactPoint contact = col.contacts[0];
            bloodParticle = (GameObject)Instantiate(bloodPrefab, contact.point, Quaternion.identity);
            bloodParticle.transform.parent = transform;
            bloodParticle.transform.localPosition = new Vector3(0, 0, 0);
            StartCoroutine(StopBlood());

            if (enemyMaster.nextGetDamage < Time.time) /// for slow down the death == increase the health
            {
                enemyMaster.nextGetDamage = Time.time + enemyMaster.getDamageRate;
                enemyMaster.CallEventEnemyHealthDeduction(weaponScript.damage);
            }
            canTakeDamage = false;
        }

    }

    void OnCollisionExit(Collision col)
    {
        if (col.collider.gameObject.tag == "PlayerWeapon" && this.enabled)
        {
            canTakeDamage = true;
        }
    }

    IEnumerator StopBlood()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(bloodParticle);
    }

    public void DisableThis()
    {
        if (bloodParticle != null)
        {
            Destroy(bloodParticle);
        }
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;
        this.enabled = false;
    }

}