using System;
using UnityEngine;

[Flags]
public enum MovementLockSource
{
    None = 0,
    Action = 1 << 0,
    Reaction = 1 << 1,
    External = 1 << 2
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterDisplacement))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private LocomotionData locomotionData;

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private CharacterInputSource inputSource;
    [SerializeField] private CharacterDisplacement displacement;

    [Header("Runtime (debug)")]
    [SerializeField] private MovementRuntimeData runtimeData;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float groundCheckRadiusMultiplier = 0.95f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugGizmos = true;

    // STATE
    private MovementLockSource movementLocks = MovementLockSource.None;
    private MovementCalculator calculator;

    // PUBLIC API
    public LocomotionData LocomotionData => locomotionData;
    public CharacterController Controller => controller;
    public MovementRuntimeData RuntimeData => runtimeData;
    public float EffectiveVerticalVelocity
    {
        get
        {
            if (displacement != null && displacement.IsDisplacing)
                return displacement.CurrentFrameVerticalVelocity;
            return runtimeData.gravitytVerticalVelocity;
        }
    }

    public bool IsGrounded =>
        (controller != null && controller.isGrounded) || ProbeGrounded();

    public bool IsHorizontalMovementLocked => movementLocks != MovementLockSource.None;
    public bool IsHorizontalMovementEnabled => movementLocks == MovementLockSource.None;
    public CharacterDisplacement Displacement => displacement;
    public bool IsDisplacing => displacement != null && displacement.IsDisplacing;

    // LIFECYCLE
    private void Reset()
    {
        controller = GetComponent<CharacterController>();
        displacement = GetComponent<CharacterDisplacement>();
    }

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (displacement == null) displacement = GetComponent<CharacterDisplacement>();

        ValidateSetup();

        runtimeData = new MovementRuntimeData();
        calculator = new MovementCalculator(locomotionData);
    }

    private void Update()
    {
        SimulateMovement(Time.deltaTime);
    }

    // SIMULATION 
    private void SimulateMovement(float deltaTime)
    {
        if (controller == null || calculator == null)
            return;

        runtimeData.isGrounded = controller.isGrounded;

        Vector3 displacementDelta = displacement != null
            ? displacement.SimulateDisplacement(deltaTime)
            : Vector3.zero;

        Vector3 rawInput = inputSource != null ? inputSource.GetMovementDirection() : Vector3.zero;
        bool blocksByDisplacement = displacement != null && displacement.BlocksHorizontalMovement;

        Vector3 inputDirection = calculator.FilterInput(
            rawInput,
            IsHorizontalMovementLocked,
            blocksByDisplacement);

        runtimeData.desiredVelocity = calculator.ComputeDesiredVelocity(inputDirection);

        // HORIZONTAL
        Vector3 newHorizontalVelocity = calculator.ComputeHorizontalVelocity(
            runtimeData.velocity,
            runtimeData.desiredVelocity,
            runtimeData.isGrounded,
            deltaTime);

        // VERTICAL
        bool useGravity = displacement == null || displacement.UsesGravity;
        bool hitCeiling = (controller.collisionFlags & CollisionFlags.Above) != 0;

        float displacementVertVel = displacement != null
            ? displacement.CurrentFrameVerticalVelocity
            : 0f;

        float newVerticalVelocity = calculator.ComputeVerticalVelocity(
            runtimeData.gravitytVerticalVelocity,
            useGravity,
            runtimeData.isGrounded,
            hitCeiling,
            displacementVertVel,
            deltaTime,
            out float effectiveVertical);

        runtimeData.gravitytVerticalVelocity = newVerticalVelocity;
        runtimeData.efectiveVerticalVelocity = effectiveVertical;
        runtimeData.velocity = new Vector3(
            newHorizontalVelocity.x,
            newVerticalVelocity,
            newHorizontalVelocity.z);

        // 6. APPLY + ROTATE (Unity)
        ApplyMovement(deltaTime, displacementDelta);
    }

    private void ApplyMovement(float deltaTime, Vector3 displacementDelta)
    {
        Vector3 normalMovement =
            new Vector3(
                runtimeData.velocity.x,
                runtimeData.gravitytVerticalVelocity,
                runtimeData.velocity.z
            ) * deltaTime;

        Vector3 finalMovement = normalMovement + displacementDelta;

        if (controller.enabled)
            controller.Move(finalMovement);

        RotateTowardsMovement(deltaTime);
    }

    private void RotateTowardsMovement(float deltaTime)
    {
        Vector3 horizontalVelocity = runtimeData.velocity;
        horizontalVelocity.y = 0f;

        if (horizontalVelocity.sqrMagnitude < 0.0001f)
            return;

        float newAngle = calculator.ComputeRotation(
               transform.eulerAngles.y,
               horizontalVelocity,
               ref runtimeData.rotationVelocity);

        transform.rotation = Quaternion.Euler(0f, newAngle, 0f);
    }

    // GROUND PROBE
    private bool ProbeGrounded()
    {
        if (controller == null) return false;

        float radius = controller.radius * groundCheckRadiusMultiplier;
        Vector3 origin = transform.position + controller.center + Vector3.up * Mathf.Max(0f, controller.height * 0.5f - radius);
        float castDistance = groundCheckDistance + 0.05f;

        return Physics.SphereCast(
            origin, radius, Vector3.down, out _,
            castDistance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    // PUBLIC CONTROL API
    public void SetMovementLock(MovementLockSource source, bool locked)
    {
        if (locked)
            movementLocks |= source;
        else
            movementLocks &= ~source;

        if (IsHorizontalMovementLocked)
            StopHorizontalMovement();
    }

    public void SetHorizontalMovementEnabled(bool enabled)
        => SetMovementLock(MovementLockSource.External, !enabled);

    public void StopHorizontalMovement()
    {
        runtimeData.velocity = new Vector3(
            0f,
            runtimeData.velocity.y,
            0f
        );
        runtimeData.desiredVelocity = Vector3.zero;
    }


    public bool StartDisplacement(DisplacementData data, Vector3 direction)
    {
        if (Displacement == null) return false;
        return Displacement.StartDisplacement(data, direction);
    }

    public void StopDisplacement() => Displacement?.StopDisplacement();

    public void SetInputSource(CharacterInputSource source) => inputSource = source;
    public void SetLocomotionData(LocomotionData data) => locomotionData = data;
    public void SetVerticalVelocity(float velocity)
        => runtimeData.gravitytVerticalVelocity = velocity;
    public void AddVerticalVelocity(float amount)
        => runtimeData.gravitytVerticalVelocity += amount;

    // VALIDATION + GIZMOS
    private void ValidateSetup()
    {
        if (controller == null)
            Debug.LogError($"{name}: CharacterMovement necesita un CharacterController.", this);
        if (locomotionData == null)
            Debug.LogWarning($"{name}: No hay LocomotionData asignado.", this);
        if (inputSource == null)
            Debug.LogWarning($"{name}: No hay CharacterInputSource asignado.", this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
            return;

        Vector3 origin = transform.position;

        Gizmos.color = Color.green;
        if (runtimeData.desiredVelocity.sqrMagnitude > 0.01f)
            Gizmos.DrawLine(origin, origin + runtimeData.desiredVelocity);

        Gizmos.color = Color.blue;
        if (runtimeData.velocity.sqrMagnitude > 0.01f)
            Gizmos.DrawLine(origin, origin + runtimeData.velocity);
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos)
            return;
        if (controller == null)
            return;

        Gizmos.color = Color.yellow;
        float radius = controller.radius * groundCheckRadiusMultiplier;

        Vector3 sphereOrigin = transform.position + controller.center
                   + Vector3.down * Mathf.Max(0f, controller.height * 0.5f - radius);
        
        Gizmos.DrawWireSphere(sphereOrigin, radius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(sphereOrigin, sphereOrigin + Vector3.down * groundCheckDistance);
    }
#endif
}
