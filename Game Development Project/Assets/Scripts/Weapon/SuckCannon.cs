using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SuckCannon : MonoBehaviour
{
    private PlayerController playerController = null;
    private GameObject weaponHandler = null;
    private WeaponRecoil weaponRecoil = null;
    private Vector3 origin = Vector3.zero;
    private Vector3 direction = Vector3.zero;
    private float currHitDistance = 0f;
    private bool junkFired = false;
    private float sphereRadius = 0.5f;
    private float maxDistance = 10f;

    [Header("Properties")]
    [SerializeField] private ParticleSystem suctionVFX = null;
    [SerializeField] private LayerMask projectileLayer = 0;
    public Transform firePos = null;
    public bool isSucking = false;
    public Image crosshairFire = null, crosshairSuck = null;
    public List<GameObject> currHitObject = new List<GameObject>();
    public LayerMask junkLayer = 6;
    public int currAmmo = 0, minAmmo = 0;

    [Header("Upgrades")]
    public float force = 60f;
    public int maxAmmo = 10;

    // Constants
    private const int zero = 0, one = 1;

    // Start is called before the first frame update
    void Start()
    {
        SetSuckCannonVariables();
        UpdateUI(isSucking);
    }

    void SetSuckCannonVariables()
    {
        playerController = GetComponent<PlayerController>();
        weaponHandler = Camera.main.transform.GetChild(1).gameObject;
        weaponRecoil = weaponHandler.GetComponent<WeaponRecoil>();
        firePos = weaponHandler.transform.GetChild(zero).transform.GetChild(zero).transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerController.suckCannonEquipped || playerController.lockInput) { return; } // if player doesn't have 'Suck Cannon' equipped then return

        // Assistance
        playerController.AimAssist();

        // Inputs
        SuckInput();
        FireInput();
    }

    private void FixedUpdate()
    {
        if (junkFired)
        {
            int lastElement = currHitObject.Count - one;
            Junk junkScript = currHitObject[lastElement].GetComponent<Junk>();
            if (junkScript.targeted) { return; } // cannot fire if junk is being 'sucked'

            if (junkScript.isWorldJunk) // world items
            {
                currHitObject[lastElement].SetActive(true);
                currHitObject[lastElement].GetComponent<Collider>().enabled = true;
                FireProjectile(currHitObject[lastElement]);
            }
            else // instantiated items
            {
                FireProjectile(Instantiate(currHitObject[lastElement]));
            }

            currHitObject.RemoveAt(lastElement);
        }
    }

    #region 'Suck Cannon' Logic

    public void FireProjectile(GameObject junkProjectile)
    {
        // Find exact hit position using a raycast
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // middle of the screen
        RaycastHit hit;

        // Check if ray hits something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, ~projectileLayer)) // make sure the projectiles collider DOES NOT interfere with 'targetPoint'
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(20); // just a point far away from the player

        // Get the rigidbody
        Rigidbody rigidbody = junkProjectile.GetComponent<Rigidbody>();

        // Reset the velocity for a clean shot & set the new transform position
        rigidbody.velocity = Vector3.zero;
        junkProjectile.transform.position = firePos.position;

        // Add relative force towards the 'targetPoint' & make 'junkProjectile' shot
        rigidbody.AddForce((targetPoint - firePos.position).normalized * force, ForceMode.Impulse);
        junkProjectile.GetComponent<Junk>().shot = true;
        junkFired = false;
    }

    public void FireInput()
    {
        origin = Camera.main.transform.position;
        direction = Camera.main.transform.forward;

        if (isSucking && currAmmo < maxAmmo)
        {
            suctionVFX.Play();
            RaycastHit hit;
            if (Physics.SphereCast(origin, sphereRadius, direction, out hit, maxDistance, junkLayer, QueryTriggerInteraction.UseGlobal))
            {
                GameObject hitObject = hit.transform.gameObject; // get the hit object 
                if (!currHitObject.Contains(hitObject)) // if the list does NOT contain 'hitObject'
                {
                    Junk junkScript = hitObject.GetComponent<Junk>(); // get the script from 'hitObject'

                    if (junkScript.shot || currHitObject.Count == maxAmmo) { return; } // can't suck items back up if they've been shot

                    junkScript.targeted = true; // then target the 'hitObject'
                    currHitObject.Add(hitObject);
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
            suctionVFX.Stop();
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

                if (currAmmo == currHitObject.Count) // prevents "rapid suck and shoot" - waits for the ammo count to match the object count
                {
                    // Fire items
                    junkFired = true;
                    weaponRecoil.Recoil();
                    UpdateAmmo(-one);

                    // Haptic feedback
                    if (Gamepad.current != null)
                        StartCoroutine(playerController.PlayHaptics(0.1f, 1.5f, 1.5f));

                    // Play sound
                    FindObjectOfType<AudioManager>().Play("Fire Junk");
                }
            }
        }
    }

    public void SuckInput()
    {
        if (playerController.SuckInput())
        {
            isSucking = !isSucking;
            UpdateUI(isSucking);
        }
    }

    public void UpdateUI(bool isSucking)
    {
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

    public void UpdateAmmo(int value = 0)
    {
        currAmmo += value;
        playerController.ammoText.text = currAmmo.ToString() + "/" + maxAmmo.ToString();
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        if (isSucking)
        {
            Gizmos.color = Color.magenta;
            Debug.DrawLine(origin, origin + direction * currHitDistance);
            Gizmos.DrawWireSphere(origin + direction * currHitDistance, sphereRadius);
        }
    }
}