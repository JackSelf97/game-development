using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuckCannon : MonoBehaviour
{
    private Transform cam = null;
    private PlayerController playerController = null;
    private Vector3 origin;
    private Vector3 direction;
    private float currHitDistance;

    // Cannon Properties
    [SerializeField] private bool isSucking = false;
    [SerializeField] Image crosshairFire = null, crosshairSuck = null;
    public List<GameObject> currHitObject = new List<GameObject>(); 
    public float sphereRadius = 0.5f;
    public float maxDistance = 5f;
    public LayerMask junkLayer = 6;
    public GameObject firePos;

    [Header("Upgrades")]
    public float force = 50f;

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

        SuckState();
        origin = cam.position;
        direction = cam.forward;

        if (isSucking)
        {
            RaycastHit hit;
            if (Physics.SphereCast(origin, sphereRadius, direction, out hit, maxDistance, junkLayer, QueryTriggerInteraction.UseGlobal))
            {
                GameObject hitObject = hit.transform.gameObject;
                if (!currHitObject.Contains(hitObject))
                {
                    // suck items
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
        if (!isSucking)
        {
            if (playerController.PlayerShoot())
            {
                if (currHitObject.Count <= 0)
                {
                    Debug.Log("No ammo");

                    // push items
                    RaycastHit hit;
                    if (Physics.Raycast(origin, direction, out hit, maxDistance))
                    {
                        if (hit.rigidbody)
                            hit.transform.GetComponent<Rigidbody>().AddForce(firePos.transform.forward * force, ForceMode.Impulse);
                    }
                    return;
                }

                // fire items
                int lastElement = currHitObject.Count - 1;
                currHitObject[lastElement].SetActive(true);
                currHitObject[lastElement].transform.position = firePos.transform.position;
                currHitObject[lastElement].GetComponent<Rigidbody>().AddForce(firePos.transform.forward * force, ForceMode.Impulse);
                currHitObject.RemoveAt(lastElement);
            }
        }
    }

    public void SuckState()
    {
        if (playerController.PlayerSuck())
        {
            isSucking = !isSucking;
        }
        if (isSucking)
        {
            crosshairFire.enabled = false;
            crosshairSuck.enabled = true;
        }
        else
        {
            crosshairSuck.enabled = false;
            crosshairFire.enabled = true;
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
