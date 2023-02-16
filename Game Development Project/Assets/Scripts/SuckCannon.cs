using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class SuckCannon : MonoBehaviour
{
    public Transform cam;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private bool isSucking = false;

    public GameObject currHitObj = null; // make array
    private Vector3 origin;
    private Vector3 direction;
    public float sphereRadius;
    public float maxDistance;
    public LayerMask layerMask;
    private float currHitDistance;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Suck();
        if (isSucking)
        {
            origin = cam.position;
            direction = cam.forward;
            RaycastHit hit;
            if (Physics.SphereCast(origin, sphereRadius, direction, out hit, maxDistance, layerMask, QueryTriggerInteraction.UseGlobal))
            {
                currHitObj = hit.transform.gameObject;
                currHitDistance = hit.distance;
            }
            else
            {
                currHitDistance = maxDistance;
                currHitObj = null;
            }
        }
    }

    public void Suck()
    {
        if (playerController.PlayerShoot())
        {
            isSucking = !isSucking;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Debug.DrawLine(origin, origin + direction * currHitDistance);
        Gizmos.DrawWireSphere(origin + direction * currHitDistance, sphereRadius);
    }
}
