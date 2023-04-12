using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller = null;
    private PlayerControls playerControls = null;
    private PlayerStats playerStats = null;
    private PlayerInput playerInput = null;
    private SuckCannon suckCannon = null;

    [Header("Player Variables")]
    [SerializeField] private GameObject pauseMenu = null;
    private Vector3 playerVelocity = Vector3.zero;
    private Transform cam = null;
    private bool isPaused = false;
    public Image currCrosshair = null;
    public LayerMask interactableLayer;
    public LayerMask targetLayer;
    public bool lockInput = false;
    public bool inConversation = false;
    public bool jump;

    // Player Traits
    private Color32 crosshairColour = new Color32(189, 213, 226, 255);
    private float moveSpeed = 9f;
    private float gravityValue = -9.81f;
    private float fallMultiplier = 2.5f;
    private float slopeForce = 40;
    private float slopeForceRayLength = 5;
    private float pushPower = 2.0f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;
    private float _speed;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.1f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private float _terminalVelocity = 53.0f;

    [Header("Weapon Properties")]
    public GameObject suckCannonObj = null;
    public GameObject gravityGunObj = null;
    public Text ammoText = null;
    public bool suckCannonEquipped = false;

    [Header("Aim Assist")]
    [SerializeField] private bool targetLock = false;
    [SerializeField] private int strength = 30;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera vCamNPC = null;
    [SerializeField] private CinemachineVirtualCamera vCamPlayer = null;
    private float TopClamp = 90.0f;
    private float BottomClamp = -90.0f;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    public float currRotationSpeed = 2.0f;
    public float rotationSpeed = 0.0f;
    public GameObject CinemachineCameraTarget;
    public bool analogMovement;

    private bool IsCurrentDeviceMouse
    {
        get
        {
            #if ENABLE_INPUT_SYSTEM
            return playerInput.currentControlScheme == "Mouse";
            #else
				return false;
            #endif
        }
    }

    [Header("Interaction & UI")]
    [SerializeField] private UIManager UIman = null;
    [SerializeField] private GameObject playerUI = null;
    [SerializeField] private Button restartButton = null;
    [SerializeField] private GameObject interactionBox = null;
    [SerializeField] private Text interactionText = null;
    [SerializeField] private GameObject ammoBox = null;
    [SerializeField] private GameObject healthBar = null;
    public Button continueButton = null;

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
        SetPlayerVariables();
    }

    void SetPlayerVariables()
    {
        playerStats = GetComponent<PlayerStats>();
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        suckCannon = GetComponent<SuckCannon>();

        // set the camera speed
        cam = Camera.main.transform;
        rotationSpeed = currRotationSpeed; 

        // set the lock state
        Cursor.lockState = CursorLockMode.Locked;
        lockInput = false;
        
        // set the ammo
        ammoText.text = suckCannon.currAmmo + "/" + suckCannon.maxAmmo;

        // set the UI
        isPaused = false;
    }

    void Update()
    {
        // Checks
        GroundCheck();

        // Inputs
        PlayerJump();
        PlayerInteraction();
        PlayerSwitch();
        CheckPause();
    }

    void FixedUpdate()
    {
        Move(); 
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void Move()
    {
        if (!lockInput)
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = moveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (GetPlayerMovement() == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = analogMovement ? GetPlayerMovement().magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(GetPlayerMovement().x, 0.0f, GetPlayerMovement().y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (GetPlayerMovement() != Vector2.zero)
            {
                // move
                inputDirection = transform.right * GetPlayerMovement().x + transform.forward * GetPlayerMovement().y;
            }

            // Move the player
            controller.Move(inputDirection.normalized * (moveSpeed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            #region Slope & Jumping

            // Slope movement
            if (GetPlayerMovement() != Vector2.zero && OnSlope())
            {
                controller.Move(Vector3.down * controller.height / 2 * slopeForce * Time.fixedDeltaTime);
            }

            // Jump effect
            if (playerVelocity.y < 0f) // if player is falling
            {
                playerVelocity += Vector3.up * gravityValue * (fallMultiplier - 1) * Time.fixedDeltaTime; // for a 'non-floaty' jump
            }

            #endregion
        }
    }

    private void CameraRotation()
    {
        if (!lockInput)
        {
            // if there is an input
            if (GetMouseDelta().sqrMagnitude >= _threshold)
            {
                //Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += GetMouseDelta().y * currRotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = GetMouseDelta().x * currRotationSpeed * deltaTimeMultiplier;

                // clamp our pitch rotation
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                // Update Cinemachine camera target pitch
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * _rotationVelocity);
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
                StartCoroutine(SwitchWeapons(gravityGunObj, suckCannonObj));
                ammoText.text = suckCannon.currAmmo + "/" + suckCannon.maxAmmo;
                playerControls.Player.Suck.Enable();
                return;
            }
            if (!suckCannonEquipped)
            {
                if (suckCannon.isSucking)
                {
                    suckCannon.isSucking = false;
                    suckCannon.UpdateUI(suckCannon.isSucking);
                    playerControls.Player.Suck.Disable();
                }

                StartCoroutine(SwitchWeapons(suckCannonObj, gravityGunObj));
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
        FindObjectOfType<AudioManager>().Play("Weapon Switch");
    }

    public void GroundCheck()
    {
        Grounded = controller.isGrounded;
        if (Grounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            jump = false;
        }
    }

    public bool OnSlope()
    {
        if (jump) { return false; }

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
        if (Grounded && !lockInput)
        {
            jump = true;

            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (JumpInput() && _jumpTimeoutDelta <= 0.0f && jump)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    public void PlayerInteraction()
    {
        RaycastHit hit;
        const float rayLength = 3;

        Debug.DrawRay(cam.position, cam.forward.normalized * rayLength, Color.cyan);
        if (Physics.Raycast(cam.position, cam.forward.normalized, out hit, rayLength, interactableLayer))
        {
            ChangeCrosshairColour(Color.cyan);

            #region Ammo Box (Junk Container)

            if (hit.transform.CompareTag("Container"))
            {
                InteractionUI(true, "LOOT");
                if (InteractInput() && suckCannonObj.activeSelf)
                {
                    JunkContainer container = hit.transform.GetComponent<JunkContainer>();

                    if (suckCannon.currAmmo < suckCannon.maxAmmo && container.currAmmo > container.minCapacity) // if the SC ammo is less than its total capacity && the container has ammo
                    {
                        int reloadAmount = suckCannon.maxAmmo - suckCannon.currAmmo; // how many junk items are needed to reach its max
                        reloadAmount = (container.currAmmo - reloadAmount) >= 0 ? reloadAmount : container.currAmmo; // get as much ammo as I need to refill 

                        suckCannon.currAmmo += reloadAmount; // add the reloadAmount to the SC [int]
                        container.UpdateContainerAmmo(-reloadAmount); // minus the reloadAmount from the container [int]

                        for (int i = 0; i < reloadAmount; i++)
                        {
                            suckCannon.currHitObject.Add(container.instantiatedJunk); // add the reloadAmount to the SC [GameObject]
                        }

                        // update SC Ammo
                        suckCannon.UpdateAmmo();

                        // play animation
                        suckCannonObj.GetComponent<Animator>().SetTrigger("IsReloading");

                        // play sound
                        FindObjectOfType<AudioManager>().Play("Loot");
                    }
                }
            }

            #endregion

            if (hit.transform.CompareTag("Health Container"))
            {
                InteractionUI(true, "HEAL");
                if (InteractInput())
                {
                    HealthContainer healthContainer = hit.transform.GetComponent<HealthContainer>();

                    if (healthContainer.uses <= 0) { return; }
                    else if (playerStats.currHP < playerStats.maxHP)
                    {
                        playerStats.currHP = playerStats.maxHP; // set the currHP to the maxHP
                        playerStats.healthBar.SetHealth(playerStats.currHP); // update healthBar UI

                        healthContainer.UpdateContainerStatus(-1); // minus the 'uses' from the health container [int]

                        // play sound
                        FindObjectOfType<AudioManager>().Play("Heal");
                    }
                }
            }

            if (hit.transform.CompareTag("NPC"))
            {
                if (!inConversation)
                    InteractionUI(true, "TALK");

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

            if (hit.transform.CompareTag("Exit"))
            {
                InteractionUI(true, "EXIT");
                if (InteractInput())
                {
                    UIman.GoToScene("Main Menu"); // roll credits...
                }
            }
        }
        else
        {
            if (!targetLock)
                ChangeCrosshairColour(Color.white); // this handles the default colour

            InteractionUI(false);
        }
    }

    public void ConversationCheck()
    {
        if (inConversation)
        {
            UIController(true, false);
            playerStats.capsuleMesh.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            InteractionUI(false);
        }
        else
        {
            UIController(false, true);
            playerStats.capsuleMesh.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void InteractionUI(bool state, string text = null)
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
                pauseMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                restartButton.Select(); 
                return;
            }
            else
            {
                Time.timeScale = 1f;
                if (!inConversation)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    EventSystem.current.SetSelectedGameObject(null);
                }
                else
                {
                    continueButton.Select();
                }
                UIman.Back();
                pauseMenu.SetActive(false);
            }
        }
    }

    private void ChangeCrosshairColour(Color32 colour)
    {
        currCrosshair.color = colour;
    }

    public void UIController(bool input, bool state)
    {
        lockInput = input;

        // Control the UI
        currCrosshair.enabled = state;
        healthBar.SetActive(state);
        ammoBox.SetActive(state);

        // Deactive the weapon
        if (suckCannonEquipped)
            suckCannonObj.SetActive(state);
        else
            gravityGunObj.SetActive(state);
    }

    #region Gamepad Logic (Xbox & PS4)

    public IEnumerator PlayHaptics(float seconds, float leftMotorSpeed = 0.25f, float rightMotorSpeed = 0.25f)
    {
        Gamepad.current.SetMotorSpeeds(leftMotorSpeed, rightMotorSpeed);
        yield return new WaitForSeconds(seconds);
        InputSystem.ResetHaptics();
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public void AimAssist()
    {
        RaycastHit hit;
        const int rayLength = 10;
        
        Debug.DrawRay(cam.position, cam.forward.normalized * rayLength, Color.red);
        if (Physics.Raycast(cam.position, cam.forward.normalized, out hit, rayLength, targetLayer))
        {
            ChangeCrosshairColour(Color.red);
            if (hit.transform.CompareTag("Enemy") && !targetLock) // bool from the enemy?
            {
                currRotationSpeed -= currRotationSpeed * strength / 100;
                targetLock = true;
            }
        }
        else
        {
            currRotationSpeed = rotationSpeed;
            targetLock = false;
        }
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