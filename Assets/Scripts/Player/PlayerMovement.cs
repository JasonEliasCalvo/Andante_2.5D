using UnityEngine;
using static CameraManager;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Animator animator;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 7.5f;
    public float acceleration = 15f;
    public float deceleration = 20f;

    [Header("Rotation")]
    public float rotationSmoothTime = 0.12f;
    private float rotationVelocity;

    [Header("Air Control")]
    [Range(0f, 1f)]
    public float airControlMultiplier = 0.6f;
    public float jumpForce = 6f;
    public float gravity = 7.5f;
    public float coyoteTime = 0.15f;
    public float groundedForce = -1f;

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 0.5f;
    public float dashForceStop = 0.4f;

    [Header("Input Buffer")]
    [SerializeField]
    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    public bool HasBufferedJump =>
        jumpBufferCounter > 0f;

    // Components
    [HideInInspector] public CharacterController controller;

    // Velocity
    [HideInInspector] public Vector3 horizontalVelocity;
    [HideInInspector] public float verticalVelocity;
    [HideInInspector] public float currentTargetSpeed;
    [HideInInspector] public Vector3 desiredMoveDir;

    // Input
    private PlayerControls controls;
    [HideInInspector] public Vector2 inputRaw;
    [HideInInspector] public bool dashPressed;

    // Timers
    [HideInInspector] public float coyoteCounter;
    [HideInInspector] public float dashTimer;
    [HideInInspector] public float dashCooldownTimer;

    public bool HasMovementInput => inputRaw.sqrMagnitude > 0.1f;

    #region State

    private BaseState currentState;
    private IdleState idleState;
    private WalkState walkState;
    private JumpState jumpState;
    private AirborneState airborneState;
    private DashState dashState;

    public IdleState Idle => idleState;
    public WalkState Walk => walkState;
    public JumpState Jump => jumpState;
    public AirborneState Fall => airborneState;
    public DashState Dash => dashState;

    #endregion

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        controls = new PlayerControls();

        idleState = new IdleState(this);
        walkState = new WalkState(this);
        jumpState = new JumpState(this);
        airborneState = new AirborneState(this);
        dashState = new DashState(this);
    }

    void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    void Start()
    {
        ChangeState(idleState);
    }

    private void Update()
    {
        ReadInput();

        currentState?.UpdateState();   // decide intención
        ApplyHorizontalMovement();     // calcula velocidad
        ApplyGravity();                // vertical
        HandleMovement();              // mueve

        dashCooldownTimer -= Time.deltaTime;
    }

    public void FixedUpdate()
    {
        currentState?.FixedUpdateState();
    }

    // =========================
    // MOVEMENT
    // =========================

    private void HandleMovement()
    {
        if (CameraManager.instance.currentStyle == CameraStyle.Basic)
        {
            Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(finalVelocity * Time.deltaTime);
        }
        else if (CameraManager.instance.currentStyle == CameraStyle.Topdown)
        {
            Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(finalVelocity * Time.deltaTime);
        }
        else if (CameraManager.instance.currentStyle == CameraStyle.Cinematic)
        {
        }
    }

    public void ApplyHorizontalMovement()
    {
        Vector3 targetVelocity = desiredMoveDir * currentTargetSpeed;

        float accel = desiredMoveDir.sqrMagnitude > 0.01f
            ? acceleration
            : deceleration;

        if (coyoteCounter <= 0f)
            accel *= airControlMultiplier;

        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetVelocity,
            accel * Time.deltaTime
        );
    }

    // =========================
    // GRAVITY & GROUND
    // =========================
    private void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = groundedForce;

            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
            verticalVelocity += Physics.gravity.y * gravity * Time.deltaTime;
        }
    }

    // =========================
    // HELPERS
    // =========================

    public void ReadInput()
    {
        inputRaw = controls.Gameplay.Move.ReadValue<Vector2>();
        dashPressed = controls.Gameplay.Dash.WasPressedThisFrame();

        if (controls.Gameplay.Jump.WasPressedThisFrame())
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    public void ChangeState(BaseState newState)
    {
        if (newState == currentState) return;

        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public Vector3 GetMoveDirection()
    {
        Vector3 forward = orientation.forward;
        Vector3 right = orientation.right;

        forward.y = 0;
        right.y = 0;

        return (forward * inputRaw.y + right * inputRaw.x).normalized;
    }

    public void RotateCharacter(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref rotationVelocity,
            rotationSmoothTime
        );

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    // =========================
    // API para estados
    // =========================
    public bool CanDash()
    {
        if (dashCooldownTimer > 0f) return false;
        if (controller.isGrounded) return true;

        return true;
    }

    public void ConsumeJumpBuffer()
    {
        jumpBufferCounter = 0f;
    }

    public void ResetHorizontalVelocity()
    {
        horizontalVelocity.x = 0f;
        horizontalVelocity.z = 0f;
    }
}
