using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SuckCannon : MonoBehaviour
{
    // Variables
    private PlayerController playerController = null;
    private WeaponRecoil weaponRecoil = null;
    private Vector3 origin;
    private Vector3 direction;
    private float currHitDistance;
    private const int zero = 0, one = 1;

    // Cannon Properties
    public Transform firePos = null;
    [SerializeField] private bool isSucking = false;
    [SerializeField] private Image crosshairFire = null, crosshairSuck = null;
    [SerializeField] private LayerMask projectileLayer;
    public List<GameObject> currHitObject = new List<GameObject>();
    private float sphereRadius = 0.5f;
    private float maxDistance = 5f;
    public LayerMask junkLayer = 6;
    public int currAmmo, minAmmo = 0;

    // Upgrades
    public float force = 20f;
    public int maxAmmo = 10;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        weaponRecoil = Camera.main.transform.GetChild(1).GetComponent<WeaponRecoil>();

        crosshairSuck.enabled = false;
        crosshairFire.enabled = true;
        playerController.currCrosshair = crosshairFire;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerController.suckCannonEquipped || playerController.lockInput) { return; } // if player doesn't have 'Suck Cannon' equipped then return

        // Inputs
        SuckInput();
        FireInput();
    }

    #region 'Suck Cannon' Logic

    public void FireJunk(GameObject junkProjectile) // needs to be moved to FixedUpdate()
    {
        //junkProjectile.transform.position = firePos.position;
        //junkProjectile.GetComponent<Rigidbody>().AddForce(firePos.forward * force, ForceMode.Impulse);
        //junkProjectile.GetComponent<Junk>().shot = true;


        // Find exact hit position using a raycast
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // middle of the screen
        RaycastHit hit;

        // Check if ray hits something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, ~projectileLayer)) // projectiles must be visible to the 'Weapon Camera' and make sure the projectiles collider DOES NOT interfere with 'targetPoint'
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(20); // just a point far away from the player





        // Add relative force towards the 'targetPoint'
        junkProjectile.GetComponent<Rigidbody>().velocity = Vector3.zero; // RESET THE VELOCITY
        junkProjectile.transform.position = firePos.position;
        junkProjectile.GetComponent<Rigidbody>().AddForce((targetPoint - firePos.position).normalized * force, ForceMode.Impulse);
        junkProjectile.GetComponent<Junk>().shot = true;









    }

    public void UpdateAmmo(int value = 0)
    {
        currAmmo += value;
        playerController.ammoText.text = currAmmo.ToString() + "/" + maxAmmo.ToString();
    }

    public void FireInput()
    {
        origin = playerController.cam.position;
        direction = playerController.cam.forward;

        if (isSucking && currAmmo < maxAmmo)
        {
            RaycastHit hit;
            if (Physics.SphereCast(origin, sphereRadius, direction, out hit, maxDistance, junkLayer, QueryTriggerInteraction.UseGlobal))
            {
                GameObject hitObject = hit.transform.gameObject; // get the hit object 
                if (!currHitObject.Contains(hitObject)) // if the list does NOT contain 'hitObject'
                {
                    Junk junkScript = hitObject.GetComponent<Junk>(); // get the script from 'hitObject'

                    if (junkScript.shot) { return; } // can't suck items back up if they've been shot
                    junkScript.targeted = true; // then target the 'hitObject'
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
            if (playerController.FireInput())
            {
                if (currHitObject.Count <= zero)
                {
                    Debug.Log("No ammo");

                    // Push items
                    RaycastHit hit;
                    if (Physics.Raycast(origin, direction, out hit, maxDistance))
                    {
                        if (hit.rigidbody)
                            hit.transform.GetComponent<Rigidbody>().AddForce(firePos.forward * force, ForceMode.Impulse);
                    }
                    return;
                }

                // Fire  items
                int lastElement = currHitObject.Count - one;
                Junk junkScript = currHitObject[lastElement].GetComponent<Junk>();

                if (junkScript.isWorldJunk) // world items
                {
                    currHitObject[lastElement].SetActive(true);
                    currHitObject[lastElement].GetComponent<Collider>().enabled = true;
                    FireJunk(currHitObject[lastElement]);
                }
                else // instantiated items
                {
                    FireJunk(Instantiate(currHitObject[lastElement]));
                }

                if (weaponRecoil.enabled) // recoil weapon is the script is enabled
                    weaponRecoil.Recoil();

                // Haptic Feedback
                if (Gamepad.current != null)
                    StartCoroutine(playerController.PlayHaptics(0.1f, 1.5f, 1.5f));

                currHitObject.RemoveAt(lastElement);
                UpdateAmmo(-one);
            }
        }
    }

    public void SuckInput()
    {
        if (playerController.SuckInput())
        {
            isSucking = !isSucking;
            if (isSucking)
            {
                // Update UI
                crosshairFire.enabled = false;
                crosshairSuck.enabled = true;
                playerController.currCrosshair = crosshairSuck;
            }
            else
            {
                // Update UI
                crosshairSuck.enabled = false;
                crosshairFire.enabled = true;
                playerController.currCrosshair = crosshairFire;
            }
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
