using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    [Header("Player Traits")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private bool isJumping = false;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float slopeForce;
    [SerializeField] private float slopeForceRayLength;
    [SerializeField] private float pushPower = 2.0f;

    public Transform cam;
    private PlayerControls playerControls;
    private SuckCannon SuckCannon;

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
        SuckCannon = GetComponent<SuckCannon>();
        cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        GroundCheck();
        JumpInput();
        PlayerInteraction();
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
        if (Physics.Raycast(cam.position, cam.forward.normalized, out hit, rayLength))
        {
            //if (PlayerShoot())
            //    Debug.Log("This is junk.");
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

    #endregion
}
