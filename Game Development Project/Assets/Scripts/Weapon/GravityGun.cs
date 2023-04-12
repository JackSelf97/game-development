using UnityEngine;
using UnityEngine.InputSystem;

public class GravityGun : MonoBehaviour
{
    // Variables
    private PlayerController playerController = null;
    private GameObject weaponHandler = null;
    private WeaponRecoil weaponRecoil = null;
    private bool bombFired = false;
    [SerializeField] private Transform firePos = null;
    [SerializeField] private GameObject gravityBomb = null;
    [SerializeField] private LayerMask projectileLayer;
    private float force = 15;

    // Constants
    private const int zero = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        weaponHandler = Camera.main.transform.GetChild(2).gameObject;
        weaponRecoil = weaponHandler.GetComponent<WeaponRecoil>();
        firePos = weaponHandler.transform.GetChild(zero).transform.GetChild(zero).transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.suckCannonEquipped || playerController.lockInput) { return; } // if player has 'Suck Cannon' equipped then return
        if (playerController.FireInput())
        {
            weaponRecoil.Recoil();

            //WeaponShake.wsMan.ShakeCamera(0.2f, 0.2f);

            // Haptic Feedback
            if (Gamepad.current != null)
                StartCoroutine(playerController.PlayHaptics(0.075f, 1f, 1f));

            // Play sound
            FindObjectOfType<AudioManager>().Play("Fire Gravity Bomb");

            bombFired = true;
        }
    }

    private void FixedUpdate()
    {
        if (bombFired)
            SpawnProjectile();
    }

    private void SpawnProjectile()
    {
        // Find exact hit position using a raycast
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, zero)); // middle of the screen
        RaycastHit hit;

        // Check if ray hits something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, ~projectileLayer)) // make sure the projectiles collider DOES NOT interfere with 'targetPoint'
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(9); // just a point far away from the player - if the 'gravityBombs' are slow, reduce the number so it meets the crosshair

        // Instantiate the 'Gravity Bomb'
        GameObject projectile = Instantiate(gravityBomb, firePos.position, Quaternion.identity);

        // Add relative force towards the 'targetPoint'
        projectile.GetComponent<Rigidbody>().AddForce((targetPoint - firePos.position).normalized * force, ForceMode.Impulse);
        bombFired = false;
    }
}