using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] Transform orientation;
    private bool isSprinting;
    [Header("jumping")]
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCoolDown;
    [SerializeField] float airMultiplier;
    bool readyToJump;


    [SerializeField] float groundDrag;
    [Header("GroundCheck")]
    [SerializeField] float playerHeight;
    [SerializeField] LayerMask whatIsGround;
    bool grounded;

    Vector3 moveDir;
    [SerializeField] Rigidbody RB;
    //[SerializeField] Rigidbody rigidbody;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += Jump_performed;


    }



    private void Jump_performed(InputAction.CallbackContext obj)
    {
        if (readyToJump && grounded)
        {
            readyToJump = false;
            RB.velocity = new Vector3(RB.velocity.x, 0f, RB.velocity.z);

            RB.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            Invoke(nameof(ResetJump), jumpCoolDown);
        }

    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    void Start()
    {

        RB.freezeRotation = true;
        readyToJump = true;
        isSprinting = false;


    }
    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if (grounded)
            RB.drag = groundDrag;
        else
            RB.drag = 0f;

    }
    private void FixedUpdate()
    {

        movement();
        SpeedControl();
    }


    private void movement()
    {
        float speedMultiplier = 10f;
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        moveDir = orientation.forward * inputVector.y + orientation.right * inputVector.x;
        if (grounded)
            if (playerInputActions.Player.Sprint.ReadValue<float>() > 0.1f)
                RB.AddForce(moveDir.normalized * moveSpeed * speedMultiplier * 4 * Time.deltaTime,ForceMode.Force);
            else
                RB.AddForce(moveDir.normalized * moveSpeed * speedMultiplier * Time.deltaTime,ForceMode.Force);
        else if (!grounded)
            RB.AddForce(moveDir.normalized * moveSpeed * 10f * airMultiplier);


    }
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(RB.velocity.x, 0f, RB.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            RB.velocity = new Vector3(limitedVel.x, RB.velocity.y, limitedVel.z);
        }
        Debug.Log(flatVel);
    }





}

