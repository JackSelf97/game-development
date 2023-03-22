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
    private int chaseSpeed = 7;
    private Transform target = null;
    private NavMeshAgent navMeshAgent = null;
    private EnemyStats enemyStats = null;

    // Ragdoll Physics
    private Animator animator = null;
    private Collider[] colliders = null;
    private Rigidbody[] rigidbodies = null;
    public BoxCollider mainCollider = null;
    public GameObject hips = null;
    public GameObject hitBox = null;

    // Projectile Impact
    public int impactCount = 0, maxWeightOfImpact = 5;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        target = PlayerManager.pMan.player.transform;
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyStats = GetComponent<EnemyStats>();

        SetRagdollParts();
        TurnOffRagdoll();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!enemyStats.isAlive) { return; }

        float distance = Vector3.Distance(target.position, transform.position);
        if (distance <= lookRadius)
        {
            // chase target
            ChaseTarget();

            if (distance <= navMeshAgent.stoppingDistance)
            {
                // attack and face the target
                animator.SetBool("IsAttacking", true);
                FaceTarget();
            }
        }
    }

    void ChaseTarget()
    {
        // Set the animation
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsChasing", true);

        // Set the scripts
        GetComponent<EnemyPatrol>().enabled = false;
        navMeshAgent.speed = chaseSpeed;

        // Set the target
        navMeshAgent.SetDestination(target.position);
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 5f);
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
        hitBox.GetComponent<Collider>().enabled = true; // turn 'hitBox' collider back on
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
                    Destroy(GetComponent<EnemyPatrol>());
                    enemyStats.isAlive = false;

                    TurnOnRagdoll();
                    Destroy(gameObject, 5); // could make into shrink death
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
