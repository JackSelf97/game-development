using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// https://www.youtube.com/watch?v=xppompv1DBg&ab_channel=Brackeys
public class EnemyController : MonoBehaviour
{
    public enum EnemyType
    {
        Zombie_Runner,
    }
    public EnemyType enemyType;

    // Enemy Traits
    public float lookRadius = 10f;
    private Transform target;
    private NavMeshAgent agent;
    private EnemyStats enemyStats = null;

    // Ragdoll Physics
    private Animator animator = null;
    private Collider[] colliders = null;
    private Rigidbody[] rigidbodies = null;
    public BoxCollider mainCollider = null;
    public GameObject hips = null;

    // Projectile Impact
    public int impactCount = 0, maxWeightOfImpact = 5;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        target = PlayerManager.pMan.player.transform;
        agent = GetComponent<NavMeshAgent>();
        enemyStats = GetComponent<EnemyStats>();

        SetRagdollParts();
        TurnOffRagdoll();
    }

    // Update is called once per frame
    void Update()
    {
        if (!enemyStats.isAlive) { return; }

        float distance = Vector3.Distance(target.position, transform.position);
        if (distance <= lookRadius)
        {
            // chase target
            animator.SetBool("IsChasing", true);
            agent.SetDestination(target.position);

            if (distance <= agent.stoppingDistance)
            {
                // attack and face the target
                animator.SetBool("IsChasing", false);
                FaceTarget();
            }
        }
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    #region Ragdoll Physics

    private void SetRagdollParts()
    {
        colliders = hips.GetComponentsInChildren<Collider>(); // get all the colliders
        rigidbodies = hips.GetComponentsInChildren<Rigidbody>();
    }

    void TurnOnRagdoll()
    {
        float suckCannonForce = PlayerManager.pMan.player.GetComponent<SuckCannon>().force;
        Transform playerCam = PlayerManager.pMan.player.GetComponent<PlayerController>().cam;

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
            if (collision.gameObject.GetComponent<Junk>().shot)
            {
                int junkProjectileWeight = collision.gameObject.GetComponent<Junk>().weight;

                Debug.Log("Time to ragdoll!");
                impactCount += junkProjectileWeight;

                if (impactCount >= maxWeightOfImpact) // different junk items will hold different weight values
                {
                    TurnOnRagdoll();
                    enemyStats.isAlive = false;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
