using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // Player Classes
    private CharacterController controller = null;
    private PlayerControls playerControls = null;
    private PlayerStats playerStats = null;

    [Header("Player Variables")]
    [SerializeField] private GameObject instantiatedJunk = null;
    [SerializeField] private GameObject menu = null;
    private Vector3 playerVelocity = Vector3.zero;
    public LayerMask interactableLayer = 7;
    public Transform cam = null;
    public Image currCrosshair = null;
    public bool lockInput = false;
    public bool inConversation = false;
    public bool isGrounded = false;
    public bool isPaused = false;

    // Player Traits
    private float speed = 9f;
    private float jumpHeight = 1f;
    private float gravityValue = -9.81f;
    private float rotationSpeed = 10f;
    private float fallMultiplier = 2.5f;
    private float slopeForce = 40;
    private float slopeForceRayLength = 5;
    private float pushPower = 2.0f;
    private bool isJumping = false;

    [Header("Gun Properties")]
    public GameObject suckCannon = null;
    public GameObject gravityGun = null;
    public Text ammoText = null;
    public bool suckCannonEquipped = false;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera vCamNPC = null;
    [SerializeField] private CinemachineVirtualCamera vCamPlayer = null;

    [Header("Interaction & UI")]
    [SerializeField] private GameObject playerUI = null;
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
        playerStats = GetComponent<PlayerStats>();
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        lockInput = false;
        isPaused = false;

        // set the ammo
        ammoText.text = GetComponent<SuckCannon>().currAmmo + "/" + GetComponent<SuckCannon>().maxAmmo;
    }

    void Update()
    {
        // Checks
        GroundCheck();

        // Inputs
        OnDeviceChange();
        PlayerJump();
        PlayerInteraction();
        PlayerSwitch();
        CheckPause();
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

    public void PlayerSwitch()
    {
        if (ScrollInput() > 0 || ScrollInput() < 0 || SwitchInput())
        {
            suckCannonEquipped = !suckCannonEquipped;

            // switch weapons and update the ammo UI
            if (suckCannonEquipped)
            {
                StartCoroutine(SwitchWeapons(gravityGun, suckCannon));
                ammoText.text = GetComponent<SuckCannon>().currAmmo + "/" + GetComponent<SuckCannon>().maxAmmo;
            }
            if (!suckCannonEquipped)
            {
                StartCoroutine(SwitchWeapons(suckCannon, gravityGun));
                ammoText.text = "\u221E"; // infinite symbol
            }
        }
    }

    IEnumerator SwitchWeapons(GameObject weapon1, GameObject weapon2)
    {
        // first weapon goes down
        weapon1.GetComponent<Animator>().SetBool("IsSwitching", true);
        yield return new WaitForSeconds(0.3f);
        weapon1.SetActive(false);

        // second weapon comes up
        weapon2.SetActive(true);
        weapon2.GetComponent<Animator>().SetBool("IsSwitching", false);
    }

    public void GroundCheck()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
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

    public void PlayerJump()
    {
        // changes the height position of the player..
        if (JumpInput() && isGrounded && !lockInput)
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

                if (InteractInput() && suckCannon.activeSelf)
                {
                    JunkContainer container = hit.transform.GetComponent<JunkContainer>();
                    SuckCannon suckCannonScript = GetComponent<SuckCannon>();

                    if (suckCannonScript.currAmmo < suckCannonScript.maxAmmo && container.currAmmo > container.minCapacity) // if the SC ammo is less than its total capacity && the container has ammo
                    {
                        int reloadAmount = suckCannonScript.maxAmmo - suckCannonScript.currAmmo; // how many junk items are needed to reach its max
                        reloadAmount = (container.currAmmo - reloadAmount) >= 0 ? reloadAmount : container.currAmmo; // get as much ammo as I need to refill 

                        suckCannonScript.currAmmo += reloadAmount; // add the reloadAmount to the SC [int]
                        container.UpdateContainerAmmo(-reloadAmount); // minus the reloadAmount from the container [int]

                        for (int i = 0; i < reloadAmount; i++)
                        {
                            suckCannonScript.currHitObject.Add(instantiatedJunk); // add the reloadAmount to the SC [GameObject]
                        }

                        // update SC UI
                        suckCannonScript.UpdateAmmo();
                        
                        // play animation
                        suckCannon.GetComponent<Animator>().SetTrigger("IsReloading");
                    }
                }
            }

            #endregion

            if (hit.transform.CompareTag("Health Container"))
            {
                Interaction(true, "[E] HEAL");

                if (InteractInput())
                {
                    HealthContainer healthContainer = hit.transform.GetComponent<HealthContainer>();

                    if (healthContainer.uses <= 0) { return; }
                    else if (playerStats.currHP < playerStats.maxHP) 
                    {
                        playerStats.currHP = playerStats.maxHP; // set the currHP to the maxHP
                        playerStats.healthBar.SetHealth(playerStats.currHP); // update healthBar UI

                        healthContainer.UpdateContainerStatus(-1); // minus the 'uses' from the health container [int]
                    }
                }
            }

            if (hit.transform.CompareTag("NPC"))
            {
                if (!inConversation)
                    Interaction(true, "[E] TALK");

                if (InteractInput())
                {
                    var NPC = hit.transform.GetComponent<NPC>();
                    NPC.TriggerDialogue();
                    inConversation = true;

                    // Update NPC camera
                    vCamNPC.Follow = hit.transform.GetChild(0).transform;
                    vCamNPC.LookAt = hit.transform.GetChild(0).transform;

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
            lockInput = true;
            Cursor.lockState = CursorLockMode.None;
            cam.transform.GetChild(0).gameObject.SetActive(false);
            cam.transform.GetChild(1).gameObject.SetActive(false);
            GetComponentInChildren<MeshRenderer>().enabled = false; // don't really need a mesh thinking about it...
            currCrosshair.enabled = false;
            Interaction(false);
        }
        else
        {
            lockInput = false;
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

    public void CheckPause()
    {
        if (SetPause())
        {
            isPaused = !isPaused;
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
    }

    #region Gamepad Logic (Xbox & PS4)

    public IEnumerator PlayHaptics(float seconds, float leftMotorSpeed = 0.25f, float rightMotorSpeed = 0.25f)
    {
        Gamepad.current.SetMotorSpeeds(leftMotorSpeed, rightMotorSpeed);
        yield return new WaitForSeconds(seconds);
        InputSystem.ResetHaptics();
    }

    void OnDeviceChange()
    {
        // Changes speed of the camera depending on device
        CinemachinePOV playerPOV = vCamPlayer.GetCinemachineComponent<CinemachinePOV>();
        const float mouseSpeed = 1, controllerSpeed = 5.5f;

        InputSystem.onDeviceChange +=
        (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    // New Device.
                    playerPOV.m_VerticalAxis.m_MaxSpeed = 4f;
                    playerPOV.m_HorizontalAxis.m_MaxSpeed = controllerSpeed;
                    break;
                case InputDeviceChange.Disconnected:
                    // Device got unplugged.
                    playerPOV.m_VerticalAxis.m_MaxSpeed = mouseSpeed;
                    playerPOV.m_HorizontalAxis.m_MaxSpeed = mouseSpeed;
                    break;
                case InputDeviceChange.Reconnected:
                    // Plugged back in.
                    break;
                case InputDeviceChange.Removed:
                    // Remove from Input System entirely; by default, Devices stay in the system once discovered.
                    break;
                default:
                    // See InputDeviceChange reference for other event types.
                    break;
            }
        };
    }

    #endregion

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

    public float ScrollInput()
    {
        return playerControls.Player.Scroll.ReadValue<float>();
    }

    public bool SwitchInput()
    {
        return playerControls.Player.Switch.triggered;
    }

    public bool JumpInput()
    {
        return playerControls.Player.Jump.triggered;
    }

    public bool SuckInput()
    {
        return playerControls.Player.Suck.triggered;
    }

    public bool FireInput()
    {
        return playerControls.Player.Fire.triggered;
    }

    public bool InteractInput()
    {
        return playerControls.Player.Interact.triggered;
    }

    public bool SetPause()
    {
        return playerControls.Player.Pause.triggered;
    }

    #endregion
}
