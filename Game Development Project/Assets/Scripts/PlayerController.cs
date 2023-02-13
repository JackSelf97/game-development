using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField] private float playerSpeed = 6f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;

    private InputManager inputManager;
    private Transform camTransform;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;
        camTransform = Camera.main.transform;
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
        Vector2 movement = inputManager.GetPlayerMovement();
        Vector3 move = new Vector3(movement.x, 0f, movement.y);
        move = camTransform.forward.normalized * move.z + camTransform.right.normalized * move.x;
        move.y = 0f;
        controller.Move(move.normalized * Time.fixedDeltaTime * playerSpeed);

        playerVelocity.y += gravityValue * Time.fixedDeltaTime;
        controller.Move(playerVelocity * Time.fixedDeltaTime);

        // player rotation
        float targetAngle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg + camTransform.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.fixedDeltaTime * rotationSpeed);
    }

    public void GroundCheck()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
    }

    public void JumpInput()
    {
        // changes the height position of the player..
        if (inputManager.PlayerJumped() && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
    }

    public void PlayerInteraction()
    {
        RaycastHit hit;
        const float rayLength = 3;

        Debug.DrawRay(camTransform.position, camTransform.forward.normalized * rayLength, Color.green);
        if (Physics.Raycast(camTransform.position, camTransform.forward.normalized, out hit, rayLength))
        {
            if (hit.collider.CompareTag("Junk"))
            {
                Debug.Log("This is junk.");
            }
        }
    }
}
