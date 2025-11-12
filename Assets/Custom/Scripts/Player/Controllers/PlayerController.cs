using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ──────────────────────────────────────
    //  SETTINGS
    // ──────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 7f;
    [SerializeField] private float _jumpHeight = 2.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    // ──────────────────────────────────────
    //  PUBLIC READ-ONLY
    // ──────────────────────────────────────
    public float Speed => _currentSpeed;
    public float SpeedSprint => _sprintSpeed;
    public float JumpHeight => _jumpHeight;

    // ──────────────────────────────────────
    //  EVENTS
    // ──────────────────────────────────────
    public UnityEvent OnJump;
    public UnityEvent OnLand;

    // ──────────────────────────────────────
    //  PRIVATE FIELDS
    // ──────────────────────────────────────
    private CharacterController _controller;
    private PlayerInputActions _input;
    private Camera _mainCam;

    private Vector3 _velocity;
    private bool _isGrounded;
    private float _currentSpeed;   // walk or sprint

    // ──────────────────────────────────────
    //  UNITY CALLBACKS
    // ──────────────────────────────────────
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = new PlayerInputActions();

        // expose the serialized values (so other scripts can read them)
        _currentSpeed = _walkSpeed;
    }

    private void Start()
    {
        _mainCam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable() => _input.Player.Enable();
    private void OnDisable() => _input.Player.Disable();

    private void Update()
    {
        GroundCheck();
        HandleSprint();
        MoveHorizontal();
        HandleJump();
        ApplyGravity();
        RotateTowardsCamera();
    }

    // ──────────────────────────────────────
    //  GROUND CHECK
    // ──────────────────────────────────────
    private void GroundCheck()
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (_isGrounded && !wasGrounded) OnLand.Invoke();
        if (!_isGrounded && wasGrounded) OnJump.Invoke();

        // Small downward force when on ground to keep the controller glued
        if (_isGrounded && _velocity.y < 0f) _velocity.y = -2f;
    }

    // ──────────────────────────────────────
    //  SPRINT
    // ──────────────────────────────────────
    private void HandleSprint()
    {
        // Sprint is a **hold** button – we read it every frame
        bool sprintPressed = _input.Player.Sprint.IsPressed();

        // Optional: only allow sprint when moving forward
        Vector2 move = _input.Player.Move.ReadValue<Vector2>();
        bool movingForward = move.y > 0.1f;

        _currentSpeed = (sprintPressed && movingForward) ? _sprintSpeed : _walkSpeed;
    }

    // ──────────────────────────────────────
    //  HORIZONTAL MOVEMENT
    // ──────────────────────────────────────
    private void MoveHorizontal()
    {
        Vector2 input = _input.Player.Move.ReadValue<Vector2>();

        // Build direction relative to player (already oriented toward camera)
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        _controller.Move(move * _currentSpeed * Time.deltaTime);
    }

    // ──────────────────────────────────────
    //  JUMP
    // ──────────────────────────────────────
    private void HandleJump()
    {
        if (_input.Player.Jump.triggered && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y);
            OnJump.Invoke();
        }
    }

    // ──────────────────────────────────────
    //  GRAVITY
    // ──────────────────────────────────────
    private void ApplyGravity()
    {
        _velocity.y += Physics.gravity.y * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    // ──────────────────────────────────────
    //  CAMERA-ORIENTED ROTATION
    // ──────────────────────────────────────
    private void RotateTowardsCamera()
    {
        if (_mainCam == null) return;

        Vector3 camForward = _mainCam.transform.forward;
        camForward.y = 0f; // keep player upright

        if (camForward.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(camForward);
        }
    }
}