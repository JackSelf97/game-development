using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class SuckCannon : MonoBehaviour
{
    public Transform cam;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private bool isSucking = false;

    public List<GameObject> currHitObject = new List<GameObject>(); // make array
    private Vector3 origin;
    private Vector3 direction;
    public float sphereRadius;
    public float maxDistance;
    public LayerMask layerMask;
    private float currHitDistance;
    public float force = 50f;
    public GameObject firePos;

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
                GameObject hitObject = hit.transform.gameObject;
                if (!currHitObject.Contains(hitObject))
                {
                    currHitObject.Add(hitObject);
                    hitObject.SetActive(false);
                    currHitDistance = hit.distance;
                }
            }
            else
            {
                currHitDistance = maxDistance;
            }
        }

        if (playerController.PlayerShoot())
        {
            Debug.Log("No ammo.");
            if (currHitObject.Count <= 0) { return; }

            int lastElement = currHitObject.Count - 1;
            currHitObject[lastElement].SetActive(true);
            currHitObject[lastElement].transform.position = firePos.transform.position;
            currHitObject[lastElement].GetComponent<Rigidbody>().AddForce(firePos.transform.forward * force, ForceMode.Impulse);
            currHitObject.RemoveAt(lastElement);

        }
    }

    public void Suck()
    {
        if (playerController.PlayerSuck())
        {
            isSucking = !isSucking;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isSucking)
        {
            Gizmos.color = Color.magenta;
            Debug.DrawLine(origin, origin + direction * currHitDistance, Color.magenta);
            Gizmos.DrawWireSphere(origin + direction * currHitDistance, sphereRadius);
        }
    }
}
