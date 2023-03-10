using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuckCannon : MonoBehaviour
{
    // Variables
    private PlayerController playerController = null;
    private Vector3 origin;
    private Vector3 direction;
    private float currHitDistance;
    private const int zero = 0, one = 1;

    // Cannon Properties
    public GameObject firePos;
    [SerializeField] private bool isSucking = false;
    [SerializeField] private Image crosshairFire = null, crosshairSuck = null;
    public List<GameObject> currHitObject = new List<GameObject>();
    private float sphereRadius = 0.5f;
    private float maxDistance = 5f;
    public LayerMask junkLayer = 6;
    public int currAmmo, minAmmo = 0;

    // Upgrades
    public float force = 50f;
    public int maxAmmo = 10;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        SuckState();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerController.suckCannonEquipped || playerController.lockInput) { return; } // if player doesn't have 'Suck Cannon' equipped then return

        SuckState();
        origin = playerController.cam.position;
        direction = playerController.cam.forward;

        if (isSucking && currAmmo < maxAmmo)
        {
            RaycastHit hit;
            if (Physics.SphereCast(origin, sphereRadius, direction, out hit, maxDistance, junkLayer, QueryTriggerInteraction.UseGlobal))
            {
                GameObject hitObject = hit.transform.gameObject;
                if (!currHitObject.Contains(hitObject))
                {
                    if (hitObject.GetComponent<Junk>().shot) { return; } // can't suck items back up if they've been shot

                    // suck items
                    currHitObject.Add(hitObject);
                    hitObject.SetActive(false);
                    currHitDistance = hit.distance;
                    UpdateAmmo(one);
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
                if (currHitObject.Count <= zero)
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

                // fire  items
                int lastElement = currHitObject.Count - one;
                Debug.Log($"Firing {currHitObject[lastElement].name}");

                if (currHitObject[lastElement].GetComponent<Junk>().isWorldJunk) // world items
                {
                    currHitObject[lastElement].SetActive(true);
                    FireJunk(currHitObject[lastElement]);
                }
                else // instantiated items
                {
                    GameObject instantiatedProjectile = Instantiate(currHitObject[lastElement], firePos.transform.position, Quaternion.identity);
                    FireJunk(instantiatedProjectile);
                }

                currHitObject.RemoveAt(lastElement);
                UpdateAmmo(-one);
            }
        }
    }

    #region 'Suck Cannon' Logic

    public void FireJunk(GameObject junkProjectile)
    {
        junkProjectile.transform.position = firePos.transform.position;
        junkProjectile.GetComponent<Rigidbody>().AddForce(firePos.transform.forward * force, ForceMode.Impulse);
        junkProjectile.GetComponent<Junk>().shot = true;
    }

    public void UpdateAmmo(int value = 0)
    {
        currAmmo += value;
        playerController.ammoText.text = currAmmo.ToString() + "/" + maxAmmo.ToString();
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
            playerController.currCrosshair = crosshairSuck;
        }
        else
        {
            crosshairSuck.enabled = false;
            crosshairFire.enabled = true;
            playerController.currCrosshair = crosshairFire;
        }
    }

    #endregion

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
