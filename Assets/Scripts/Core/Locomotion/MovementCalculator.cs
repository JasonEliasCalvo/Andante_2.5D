using UnityEngine;

public class MovementCalculator
{
    private readonly LocomotionData data;
    public MovementCalculator(LocomotionData data)
    {
        this.data = data;
    }

    // INPUT
    /// <summary>
    /// Filtra el input crudo: aplica locks, descarta magnitudes pequeñas
    /// y aplana el eje Y.
    /// </summary>

    public Vector3 FilterInput(Vector3 rawInput, bool isHorizontalLocked, bool blocksByDisplacement)
    {
        if (isHorizontalLocked || blocksByDisplacement)
            return Vector3.zero;
        if (rawInput.sqrMagnitude < 0.0001f)
            return Vector3.zero;
        rawInput.y = 0f;
        return rawInput.normalized;
    }

    // DESIRED VELOCITY
    public Vector3 ComputeDesiredVelocity(Vector3 inputDirection)
    {
        return inputDirection * data.walkSpeed;
    }

    // HORIZONTAL VELOCITY 
    public Vector3 ComputeHorizontalVelocity(
        Vector3 currentVelocity,
        Vector3 desiredVelocity,
        bool isGrounded,
        float deltaTime)
    {
        Vector3 horizontal = currentVelocity;
        horizontal.y = 0f;
        bool accelerating = desiredVelocity.sqrMagnitude > 0.001f;
        float rate = accelerating
            ? (isGrounded ? data.acceleration : data.airAcceleration)
            : (isGrounded ? data.deceleration : data.airDeceleration);
        return Vector3.MoveTowards(horizontal, desiredVelocity, rate * deltaTime);
    }

    // VERTICAL VELOCITY
    /// <summary>
    /// Calcula la velocidad vertical del siguiente frame.
    /// Devuelve por out la velocidad efectiva (gravedad O desplazamiento).
    /// </summary>
    public float ComputeVerticalVelocity(
        float currentGravityVelocity,
        bool useGravity,
        bool isGrounded,
        bool hitCeiling,
        float displacementVerticalVelocity,
        float deltaTime,
        out float effectiveVertical)
    {
        if (!useGravity)
        {
            effectiveVertical = displacementVerticalVelocity;
            return displacementVerticalVelocity;
        }

        effectiveVertical = currentGravityVelocity;
        float v = currentGravityVelocity;

        if (hitCeiling && v > 0f)
            v = 0f;

        if (isGrounded && v < data.groundedVerticalVelocity)
        {
            v = data.groundedVerticalVelocity;
        }
        else if (!isGrounded)
        {
            float multiplier = v > 0f ? data.riseGravityMultiplier : data.fallGravityMultiplier;
            v += data.gravity * multiplier * deltaTime;
            v = Mathf.Max(v, -data.terminalVelocity);
        }

        effectiveVertical = v;
        return v;
    }

    // ROTATION (smooth damp)
    public float ComputeRotation(float currentAngle, Vector3 horizontalVelocity, ref float rotationVelocity)
    {
        if (horizontalVelocity.sqrMagnitude < 0.0001f)
            return currentAngle;

        float targetAngle = Mathf.Atan2(horizontalVelocity.x, horizontalVelocity.z) * Mathf.Rad2Deg;
        return Mathf.SmoothDampAngle(
            currentAngle,
            targetAngle,
            ref rotationVelocity,
            data.rotationSmoothTime);
    }
}
