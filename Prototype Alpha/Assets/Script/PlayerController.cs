using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private Transform orientation;
    [SerializeField] private CharacterController controller;
    private Vector3 moveDir;
    private Vector3 velocity;

    [Header("Jumping Parameters")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpCoolDown;
    [SerializeField] private float jumpButtonGracePeriod;

    [Header("Sprinting Parameters")]
    [SerializeField] private float groundDrag;
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float airMultiplier;
    [SerializeField] private float stamina;
    [SerializeField] private float sprintMultiplier = 2f;

    private bool grounded;
    private bool readyToJump;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        // Initialize PlayerInputActions
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += JumpPerformed;
    }

    // Jump callback
    private void JumpPerformed(InputAction.CallbackContext obj)
    {
        jumpButtonPressedTime = Time.time;

        if (readyToJump && Time.time - lastGroundedTime <= jumpButtonGracePeriod)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            controller.Move(velocity * Time.deltaTime);

            readyToJump = false;
            jumpButtonPressedTime = null;
            lastGroundedTime = null;
            Invoke(nameof(ResetJump), jumpCoolDown);
        }
    }

    // Reset the ability to jump
    private void ResetJump()
    {
        readyToJump = true;
    }

    // Handle player movement
    private void Movement()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        moveDir = orientation.forward * inputVector.y + orientation.right * inputVector.x;

        if (playerInputActions.Player.Sprint.ReadValue<float>() > 0.1f && grounded && stamina != 0)
        {
            controller.Move(moveDir.normalized * moveSpeed * sprintMultiplier * Time.deltaTime);
        }
        else
        {
            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }
    }

    private void Update()
    {
        // Check if the player is grounded
        grounded = Physics.CheckSphere(transform.position, playerHeight * 0.5f + 0.2f, whatIsGround);
        ResetJump();

        // Apply drag when grounded
        if (grounded && velocity.y < 0)
        {
            velocity.y = -groundDrag;
        }

        // Update last grounded time
        if (grounded)
        {
            lastGroundedTime = Time.time;
        }
    }

    private void FixedUpdate()
    {
        // Handle player movement and apply gravity
        Movement();
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}