using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // Ragdoll Physics
    private Animator animator = null;
    private Collider[] colliders = null;
    private Rigidbody[] rigidbodies = null;
    public CapsuleCollider mainCollider = null;
    public GameObject hips = null;
    public GameObject player = null;

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

    private void OnCollisionEnter(Collision collision) // getting hit by junk items
    {
        if (collision.gameObject.layer == 6)
        {
            Debug.Log("Time to ragdoll!");
            TurnOnRagdoll();
        }
    }
}
