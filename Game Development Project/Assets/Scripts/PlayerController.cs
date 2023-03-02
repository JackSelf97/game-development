using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Variables
    private CharacterController controller;
    private PlayerControls playerControls;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private bool isJumping = false;
    private float gravityValue = -9.81f;
    public LayerMask interactableLayer = 7;
    public Transform cam;
    public Image currCrosshair;
    public GameObject junk;

    [Header("Player Traits")]
    private float speed = 6f;
    private float jumpHeight = 1f;
    private float rotationSpeed = 10f;
    private float fallMultiplier = 2.5f;
    private float slopeForce;
    private float slopeForceRayLength;
    private float pushPower = 2.0f;

    // Guns
    public GameObject gravityGun = null;
    public GameObject suckCannon = null;
    public Text ammoText = null;
    public bool suckCannonEquipped = false;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        ammoText = transform.GetChild(2).transform.GetChild(3).GetComponent<Text>(); // could change
    }

    void Update()
    {
        GroundCheck();
        JumpInput();
        PlayerInteraction();
        SwitchGun();
    }

    void FixedUpdate()
    {
        ControllerMovement();   
    }

    public void ControllerMovement()
    {
        Vector2 movement = GetPlayerMovement();
        Vector3 move = new Vector3(movement.x, 0f, movement.y);
        move = cam.forward.normalized * move.z + cam.right.normalized * move.x;
        move.y = 0f;
        controller.Move(move.normalized * Time.fixedDeltaTime * speed);

        playerVelocity.y += gravityValue * Time.fixedDeltaTime;
        controller.Move(playerVelocity * Time.fixedDeltaTime);

        // player rotation
        float targetAngle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.fixedDeltaTime * rotationSpeed);

        if (movement != Vector2.zero && OnSlope()) // if player is moving and on a slope
        {
            controller.Move(Vector3.down * controller.height / 2 * slopeForce * Time.fixedDeltaTime);
            Debug.Log("I'm on a slope!");
        }

        // jump effect
        if (playerVelocity.y < 0f) // if player is falling
        {
            playerVelocity += Vector3.up * gravityValue * (fallMultiplier - 1) * Time.fixedDeltaTime; // for a 'non-floaty' jump
        }
    }

    public void SwitchGun()
    {
        if (PlayerSwitchGun() > 0 || PlayerSwitchGun() < 0)
        {
            suckCannonEquipped = !suckCannonEquipped;
        }
        if (suckCannonEquipped)
        {
            suckCannon.SetActive(true);
            gravityGun.SetActive(false);
            return;
        }
        if (!suckCannonEquipped)
        {
            suckCannon.SetActive(false);
            gravityGun.SetActive(true);
        }
    }

    public void GroundCheck()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            isJumping = false;
        }
    }

    public bool OnSlope()
    {
        if (isJumping) { return false; }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, controller.height / 2 * slopeForceRayLength))
        {
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    public void JumpInput()
    {
        // changes the height position of the player..
        if (PlayerJump() && groundedPlayer)
        {
            isJumping = true;
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
    }

    public void PlayerInteraction()
    {
        RaycastHit hit;
        const float rayLength = 3;

        Debug.DrawRay(cam.position, cam.forward.normalized * rayLength, Color.green);
        if (Physics.Raycast(cam.position, cam.forward.normalized, out hit, rayLength, interactableLayer))
        {
            currCrosshair.color = Color.yellow;
            if (PlayerInteract() && hit.transform.CompareTag("Container"))
            {
                Debug.Log("Trying to access container");
                JunkContainer container = hit.transform.GetComponent<JunkContainer>();
                SuckCannon suckCannonScript = GetComponent<SuckCannon>();
                if (suckCannonScript.currAmmo < suckCannonScript.maxAmmo && container.currAmmo > 0) // if the SC ammo is less than its total capacity && the container has ammo
                {
                    int transferAmmo = suckCannonScript.maxAmmo - suckCannonScript.currAmmo; // get as much ammo as I need to refill the clip
                    Debug.Log($"Getting ammo...{transferAmmo}");

                    suckCannonScript.currAmmo += transferAmmo; // transfer that amount to the SC [int]
                    container.currAmmo -= transferAmmo;

                    for (int i = 0; i < transferAmmo; i++)
                    {
                        Debug.Log("Adding ammo...");
                        suckCannonScript.currHitObject.Add(junk); // transfer that amount to the SC [GameObject]
                    }

                    suckCannonScript.UpdateAmmo(); // update the UI
                    container.UpdateContainerAmmo();

                }
            }
        }
        else
        {
            currCrosshair.color = Color.white;
        }
    }

    // this script pushes all rigidbodies that the character touches
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
        {
            return;
        }

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3)
        {
            return;
        }

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * pushPower;
    }


    #region Player Inputs

    public Vector2 GetPlayerMovement()
    {
        return playerControls.Player.Move.ReadValue<Vector2>();
    }
    public Vector2 GetMouseDelta()
    {
        return playerControls.Player.Look.ReadValue<Vector2>();
    }

    public float PlayerSwitchGun()
    {
        return playerControls.Player.SwitchGun.ReadValue<float>();
    }

    public bool PlayerJump()
    {
        return playerControls.Player.Jump.triggered;
    }

    public bool PlayerSuck()
    {
        return playerControls.Player.Suck.triggered;
    }

    public bool PlayerShoot()
    {
        return playerControls.Player.Shoot.triggered;
    }

    public bool PlayerInteract()
    {
        return playerControls.Player.Interact.triggered;
    }

    #endregion
}
