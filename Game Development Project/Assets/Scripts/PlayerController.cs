using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Player Variables
    private CharacterController controller = null;
    private PlayerControls playerControls = null;
    private Vector3 playerVelocity = Vector3.zero;
    private bool isJumping = false;
    private float gravityValue = -9.81f;
    public LayerMask interactableLayer = 7;
    public Transform cam = null;
    public Image currCrosshair = null;
    public bool lockInput = false;
    public bool inConversation = false;
    [SerializeField] private bool groundedPlayer = false;
    [SerializeField] private GameObject instantiatedJunk = null;
    public bool isPaused = false;
    [SerializeField] private GameObject menu = null;

    // Player Traits
    private float speed = 8f;
    private float jumpHeight = 1f;
    private float rotationSpeed = 10f;
    private float fallMultiplier = 2.5f;
    private float slopeForce = 40;
    private float slopeForceRayLength = 5;
    private float pushPower = 2.0f;

    // Gun Variables
    public GameObject gravityGun = null;
    public GameObject suckCannon = null;
    public Text ammoText = null;
    public bool suckCannonEquipped = false;

    // Cinemachine
    [SerializeField] private CinemachineVirtualCamera camNPC = null;

    // Interaction
    [SerializeField] private GameObject interactionBox = null;
    [SerializeField] private Text interactionText = null;

    private void Awake()
    {
        Application.targetFrameRate = 120;
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
        lockInput = false;

        // set the ammo
        ammoText.text = GetComponent<SuckCannon>().currAmmo + "/" + GetComponent<SuckCannon>().maxAmmo;
    }

    void Update()
    {
        // Checks
        GroundCheck();

        // Inputs
        JumpInput();
        PlayerInteraction();
        SwitchGun();
        Pause();
    }

    void FixedUpdate()
    {
        ControllerMovement();   
    }

    public void ControllerMovement()
    {
        if (!lockInput)
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
            currCrosshair.color = Color.cyan;

            #region Ammo Box (Junk Container)

            if (hit.transform.CompareTag("Container"))
            {
                Interaction(true, "[E] LOOT");

                if (PlayerInteract())
                {
                    JunkContainer container = hit.transform.GetComponent<JunkContainer>();
                    SuckCannon suckCannonScript = GetComponent<SuckCannon>();

                    if (suckCannonScript.currAmmo < suckCannonScript.maxAmmo && container.currAmmo > container.minCapacity) // if the SC ammo is less than its total capacity && the container has ammo
                    {
                        int reloadAmount = suckCannonScript.maxAmmo - suckCannonScript.currAmmo; // how many junk items are needed to reach its max
                        reloadAmount = (container.currAmmo - reloadAmount) >= 0 ? reloadAmount : container.currAmmo; // get as much ammo as I need to refill 

                        suckCannonScript.currAmmo += reloadAmount; // add the reloadAmount to the SC [int]
                        container.currAmmo -= reloadAmount; // minus the reloadAmount from the container [int]

                        for (int i = 0; i < reloadAmount; i++)
                        {
                            suckCannonScript.currHitObject.Add(instantiatedJunk); // add the reloadAmount to the SC [GameObject]
                        }

                        // update SC & Container UI
                        suckCannonScript.UpdateAmmo();
                        container.UpdateContainerAmmo();
                    }
                }
            }

            #endregion

            if (hit.transform.CompareTag("NPC"))
            {
                if (!inConversation)
                    Interaction(true, "[E] TALK");

                if (PlayerInteract())
                {
                    var NPC = hit.transform.GetComponent<NPC>();
                    NPC.TriggerDialogue();
                    inConversation = true;
                    lockInput = true;

                    // Update NPC camera
                    camNPC.Follow = hit.transform.GetChild(0).transform;
                    camNPC.LookAt = hit.transform.GetChild(0).transform;

                    ConversationCheck();
                }
            }
        }
        else
        {
            currCrosshair.color = Color.white;
            Interaction(false);
        }
    }

    public void ConversationCheck()
    {
        if (inConversation)
        {
            Cursor.lockState = CursorLockMode.None;
            cam.transform.GetChild(0).gameObject.SetActive(false);
            cam.transform.GetChild(1).gameObject.SetActive(false);
            GetComponentInChildren<MeshRenderer>().enabled = false; // don't really need a mesh thinking about it...
            currCrosshair.enabled = false;
            Interaction(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            cam.transform.GetChild(0).gameObject.SetActive(true);
            cam.transform.GetChild(1).gameObject.SetActive(true);
            GetComponentInChildren<MeshRenderer>().enabled = true;
            currCrosshair.enabled = true;
        }
    }

    public void Interaction(bool state, string text = null)
    {
        interactionBox.SetActive(state);
        interactionText.text = text;
    }

    public void Pause()
    {
        if (SetPause())
        {
            isPaused = !isPaused;
        }
        if (isPaused)
        {
            Time.timeScale = 0f;
            menu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            return;
        }
        else
        {
            Time.timeScale = 1f;
            menu.SetActive(false);

            if (!inConversation)
                Cursor.lockState = CursorLockMode.Locked;
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

    public bool SetPause()
    {
        return playerControls.Player.Pause.triggered;
    }

    #endregion
}
