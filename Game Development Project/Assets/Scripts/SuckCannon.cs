using System.Collections.Generic;
using UnityEngine;

public class SuckCannon : MonoBehaviour
{
    private Transform cam = null;
    private PlayerController playerController = null;

    [SerializeField] private bool isSucking = false;
    public List<GameObject> currHitObject = new List<GameObject>(); 
    private Vector3 origin;
    private Vector3 direction;
    public float sphereRadius = 0.5f;
    public float maxDistance = 5f;
    public LayerMask layerMask;
    private float currHitDistance;

    [Header("Upgrades")]
    public float force = 50f;
    public GameObject firePos;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerController.suckCannonEquipped) { return; } // if player doesn't have 'Suck Cannon' equipped then return

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
            if (currHitObject.Count <= 0)
            {
                Debug.Log("No ammo.");

                // force push
                RaycastHit hit;
                if (Physics.Raycast(cam.position, cam.forward, out hit, 5, layerMask))
                {
                    Debug.Log("Push!");
                    Debug.Log(hit.transform.gameObject);
                    hit.transform.GetComponent<Rigidbody>().AddForce(firePos.transform.forward * force, ForceMode.Impulse);
                }
                return;
            }

            // suck
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
