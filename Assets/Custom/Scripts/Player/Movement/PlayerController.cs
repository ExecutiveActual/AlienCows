using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public CharacterController controller;
    public float speed = 5f;
    public float jumpHeight = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    private PlayerInputActions inputActions;

    private Camera mainCamera;


    public UnityEvent OnJump;
    public UnityEvent OnLand;
    


    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        mainCamera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
    }


    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        // Ground check
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        SetIsGrounded(Physics.CheckSphere(groundCheck.position, groundDistance, groundMask));

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Horizontal movement
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);

        // Jump
        if (inputActions.Player.Jump.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
        }

        // Gravity
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Rotate player to face camera direction
        RotatePlayerTowardsCamera();
    }

    private void RotatePlayerTowardsCamera()
    {
        if (mainCamera != null)
        {
            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0f; // Ignore the y-axis rotation

            if (cameraForward != Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(cameraForward);
                transform.rotation = newRotation;
            }
        }
    }

    private void SetIsGrounded(bool value)
    {

        if (isGrounded != value)
        {
            isGrounded = value;

            Debug.Log(value ? "Landed" : "Jumped");

            if (isGrounded)
            {
                OnLand.Invoke();
            }
            else
            {
                OnJump.Invoke();
            }
        }

    }

}