using UnityEngine;

public class GravityGun : MonoBehaviour
{
    private PlayerController playerController = null;
    private bool bombFired = false;
    private float travelSpeed = 3;
    [SerializeField] private GameObject firePos = null, gravityBomb = null;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.suckCannonEquipped || playerController.lockInput) { return; } // if player has 'Suck Cannon' equipped then return
        if (playerController.PlayerShoot())
        {
            bombFired = true;
        }
    }

    private void FixedUpdate()
    {
        if (bombFired)
            SpawnBomb();
    }

    private void SpawnBomb()
    {
        GameObject bomb = Instantiate(gravityBomb, firePos.transform.position, Quaternion.identity);
        bomb.GetComponent<Rigidbody>().AddForce(firePos.transform.forward * travelSpeed, ForceMode.Impulse);
        bombFired = false;
    }
}