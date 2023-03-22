using UnityEngine;

public class GravityGun : MonoBehaviour
{
    // Variables
    private PlayerController playerController = null;
    private WeaponRecoil weaponRecoil = null;
    private bool bombFired = false;

    [SerializeField] private Transform firePos = null;
    [SerializeField] private float outwardsForce = 3;
    [SerializeField] private GameObject gravityBomb = null;
    [SerializeField] private LayerMask projectileLayer;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        weaponRecoil = Camera.main.transform.GetChild(2).GetComponent<WeaponRecoil>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.suckCannonEquipped || playerController.lockInput) { return; } // if player has 'Suck Cannon' equipped then return
        if (playerController.PlayerShoot())
        {
            if (weaponRecoil.enabled) // recoil weapon is the script is enabled
                weaponRecoil.Recoil();

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
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // middle of the screen
        RaycastHit hit;

        // Check if ray hits something
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, ~projectileLayer)) // projectiles must be visible to the 'Weapon Camera' and make sure the projectiles collider DOES NOT interfere with 'targetPoint'
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(20); // just a point far away from the player

        // Instantiate the 'Gravity Bomb'
        GameObject projectile = Instantiate(gravityBomb, firePos.position, Quaternion.identity);

        // Add relative force towards the 'targetPoint'
        projectile.GetComponent<Rigidbody>().AddForce((targetPoint - firePos.position).normalized * outwardsForce, ForceMode.Impulse);
        bombFired = false;
    }
}