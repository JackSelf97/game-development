using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        Zombie_Runner,
    }
    public EnemyType enemyType;

    // Ragdoll Physics
    private Animator animator = null;
    private Collider[] colliders = null;
    private Rigidbody[] rigidbodies = null;
    public BoxCollider mainCollider = null;
    public GameObject hips = null;
    public GameObject player = null;

    // Projectile Impact
    public int impactCount = 0, maxWeightOfImpact = 5;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        SetRagdollParts();
        TurnOffRagdoll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Ragdoll Physics

    private void SetRagdollParts()
    {
        colliders = hips.GetComponentsInChildren<Collider>(); // get all the colliders
        rigidbodies = hips.GetComponentsInChildren<Rigidbody>();
    }

    void TurnOnRagdoll()
    {
        float suckCannonForce = player.GetComponent<SuckCannon>().force;
        Transform playerCam = player.GetComponent<PlayerController>().cam;

        animator.enabled = false;
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
            rb.AddForce(playerCam.transform.forward * suckCannonForce, ForceMode.Impulse);
        }

        mainCollider.enabled = false;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    void TurnOffRagdoll()
    {
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }

        animator.enabled = true;
        mainCollider.enabled = true;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    #endregion

    private void OnCollisionEnter(Collision collision) // getting hit by junk items
    {
        if (collision.gameObject.layer == 6)
        {
            int junkProjectileWeight = collision.gameObject.GetComponent<Junk>().weight;

            Debug.Log("Time to ragdoll!");
            impactCount += junkProjectileWeight;

            if (impactCount >= maxWeightOfImpact) // different junk items will hold different weight values
                TurnOnRagdoll();
        }
    }
}
