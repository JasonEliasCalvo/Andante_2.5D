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

    [Header("Runtime")]
    [SerializeField] private MovementRuntimeData runtimeData;

    [Header("Debug")]
    [SerializeField] private bool drawDebugGizmos = true;

    [Header("Ground Check Avanzado")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float groundCheckRadiusMultiplier = 0.95f;

    private MovementLockSource movementLocks = MovementLockSource.None;

    public LocomotionData LocomotionData => locomotionData;
    public CharacterController Controller => controller;
    public MovementRuntimeData RuntimeData => runtimeData;

    public float EffectiveVerticalVelocity
    {
        get
        {
            if (displacement != null && displacement.IsDisplacing)
                return displacement.CurrentFrameVerticalVelocity;

            else
                return runtimeData.gravitytVerticalVelocity;
        }
    }

    public bool IsGrounded =>
        (controller != null && controller.isGrounded) || ProbeGrounded();

    public bool IsHorizontalMovementLocked =>
        movementLocks != MovementLockSource.None;

    public bool IsHorizontalMovementEnabled =>
        movementLocks == MovementLockSource.None;

    public CharacterDisplacement Displacement => displacement;

    public bool IsDisplacing =>
        displacement != null && displacement.IsDisplacing;

    private void Reset()
    {
        controller = GetComponent<CharacterController>();
        displacement = GetComponent<CharacterDisplacement>();
    }

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (displacement == null)
            displacement = GetComponent<CharacterDisplacement>();

        ValidateSetup();

        runtimeData = new MovementRuntimeData();
    }

    private void Update()
    {
        SimulateMovement(Time.deltaTime);
    }

    private void SimulateMovement(float deltaTime)
    {
        if (controller == null || locomotionData == null)
            return;

        runtimeData.isGrounded = controller.isGrounded;

        Vector3 displacementDelta = displacement != null ?
            displacement.SimulateDisplacement(deltaTime) : Vector3.zero;

        Vector3 inputDirection = GetInputDirection();

        if (displacement != null && displacement.BlocksHorizontalMovement)
            inputDirection = Vector3.zero;

        bool useGravity = displacement == null || displacement.UsesGravity;

        CalculateDesiredVelocity(inputDirection);
        CalculateHorizontalVelocity(deltaTime);
        CalculateVerticalVelocity(deltaTime, useGravity);

        ApplyMovement(deltaTime, displacementDelta);
    }

    private Vector3 GetInputDirection()
    {
        if (!IsHorizontalMovementEnabled)
            return Vector3.zero;

        if (inputSource == null)
            return Vector3.zero;

        Vector3 direction = inputSource.GetMovementDirection();

        if (direction.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        direction.y = 0f;

        return direction.normalized;
    }

    private void CalculateDesiredVelocity(Vector3 direction)
    {
        runtimeData.desiredVelocity = direction * locomotionData.walkSpeed;
    }

    private void CalculateHorizontalVelocity(float deltaTime)
    {
        Vector3 currentHorizontalVelocity = runtimeData.velocity;
        currentHorizontalVelocity.y = 0f;

        Vector3 targetVelocity = runtimeData.desiredVelocity;

        float rate;

        if (targetVelocity.sqrMagnitude > 0.001f)
        {
            rate = runtimeData.isGrounded
                ? locomotionData.acceleration
                : locomotionData.airAcceleration;
        }
        else
        {
            rate = runtimeData.isGrounded
                ? locomotionData.deceleration
                : locomotionData.airDeceleration;
        }

        Vector3 newHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetVelocity,
            rate * deltaTime
        );

        runtimeData.velocity = new Vector3(
            newHorizontalVelocity.x,
            runtimeData.velocity.y,
            newHorizontalVelocity.z
        );
    }

    private void CalculateVerticalVelocity(float deltaTime, bool useGravity)
    {
        runtimeData.efectiveVerticalVelocity = EffectiveVerticalVelocity;

        if (!useGravity)
        {
            runtimeData.gravitytVerticalVelocity = runtimeData.efectiveVerticalVelocity;
            return;
        }

        if ((controller.collisionFlags & CollisionFlags.Above) != 0 && runtimeData.gravitytVerticalVelocity > 0f)
        {
            runtimeData.gravitytVerticalVelocity = 0f;
        }

        if (controller.isGrounded && runtimeData.gravitytVerticalVelocity < locomotionData.groundedVerticalVelocity)
        {
            runtimeData.gravitytVerticalVelocity = locomotionData.groundedVerticalVelocity;
        }
        else if (!controller.isGrounded)
        {
            float gravityMultiplier =
                runtimeData.gravitytVerticalVelocity > 0f
                    ? locomotionData.riseGravityMultiplier
                    : locomotionData.fallGravityMultiplier;

            float currentGravity = locomotionData.gravity * gravityMultiplier;

            runtimeData.gravitytVerticalVelocity += currentGravity * deltaTime;

            runtimeData.gravitytVerticalVelocity = Mathf.Max(
                runtimeData.gravitytVerticalVelocity,
                -locomotionData.terminalVelocity
            );
        }

        runtimeData.efectiveVerticalVelocity = runtimeData.gravitytVerticalVelocity;
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

        float targetAngle =
            Mathf.Atan2(
                horizontalVelocity.x,
                horizontalVelocity.z
            ) * Mathf.Rad2Deg;

        float angle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref runtimeData.rotationVelocity,
            locomotionData.rotationSmoothTime
        );

        transform.rotation =
            Quaternion.Euler(0f, angle, 0f);
    }

    private bool ProbeGrounded()
    {
        if (controller == null) return false;

        float radius = controller.radius * groundCheckRadiusMultiplier;
        Vector3 origin = transform.position + controller.center + Vector3.up * Mathf.Max(0f, controller.height * 0.5f - radius);
        float castDistance = groundCheckDistance + 0.05f;

        return Physics.SphereCast(origin, radius, Vector3.down, out _, castDistance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    // --- MOVEMENT CONTROL ---
    public void SetMovementLock(MovementLockSource source, bool locked)
    {
        if (locked)
            movementLocks |= source;
        else
            movementLocks &= ~source;

        if (IsHorizontalMovementLocked)
        {
            StopHorizontalMovement();
        }
    }

    public void SetHorizontalMovementEnabled(bool enabled)
    {
        SetMovementLock(MovementLockSource.External,
            !enabled
        );
    }

    public void StopHorizontalMovement()
    {
        runtimeData.velocity = new Vector3(
            0f,
            runtimeData.velocity.y,
            0f
        );

        runtimeData.desiredVelocity =
            Vector3.zero;
    }

    public bool StartDisplacement(DisplacementData data, Vector3 direction)
    {
        if (Displacement == null)
            return false;

        return Displacement.StartDisplacement(
            data,
            direction
        );
    }

    public void StopDisplacement()
    {
        Displacement?.StopDisplacement();
    }

    // --- EXTERNAL SETTERS ---
    public void SetInputSource(CharacterInputSource source)
    {
        inputSource = source;
    }

    public void SetLocomotionData(LocomotionData data)
    {
        locomotionData = data;
    }

    public void SetVerticalVelocity(float velocity)
    {
        runtimeData.gravitytVerticalVelocity = velocity;
    }

    public void AddVerticalVelocity(float amount)
    {
        runtimeData.gravitytVerticalVelocity += amount;
    }

    private void ValidateSetup()
    {
        if (controller == null)
        {
            Debug.LogError(
                $"{name}: CharacterMovement necesita un CharacterController.",
                this
            );
        }

        if (locomotionData == null)
        {
            Debug.LogWarning(
                $"{name}: No hay LocomotionData asignado.",
                this
            );
        }

        if (inputSource == null)
        {
            Debug.LogWarning(
                $"{name}: No hay CharacterInputSource asignado.",
                this
            );
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
            return;

        Vector3 origin = transform.position;

        // Input / desired direction
        Gizmos.color = Color.green;

        if (runtimeData.desiredVelocity.sqrMagnitude > 0.01f)
        {
            Gizmos.DrawLine(
                origin,
                origin + runtimeData.desiredVelocity
            );
        }

        // Current velocity
        Gizmos.color = Color.blue;

        if (runtimeData.velocity.sqrMagnitude > 0.01f)
        {
            Gizmos.DrawLine(
                origin,
                origin + runtimeData.velocity
            );
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos)
            return;
        if (controller == null)
            return;

        // Ground check
        Gizmos.color = Color.yellow;
        float radius = controller.radius * groundCheckRadiusMultiplier;
        Vector3 sphereOrigin = transform.position + controller.center + Vector3.down * Mathf.Max(0f, controller.height * 0.5f - radius);
        Gizmos.DrawWireSphere(sphereOrigin, radius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            sphereOrigin,
            sphereOrigin + Vector3.down * groundCheckDistance
        );
    }
#endif
}

[System.Serializable]
public struct MovementRuntimeData
{
    [Header("Velocity")]
    public Vector3 velocity;
    public float gravitytVerticalVelocity;
    public float efectiveVerticalVelocity;

    [Header("Desired Movement")]
    public Vector3 desiredVelocity;

    [Header("Rotation")]
    public float rotationVelocity;

    [Header("State")]
    public bool isGrounded;
}
