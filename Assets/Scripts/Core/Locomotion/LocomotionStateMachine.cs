/// <summary>
/// FSM pura: dados grounded + velocidades, devuelve Phase + SubPhase.
/// Mantiene coyote timer como estado interno (lo único mutable).
/// </summary>
public class LocomotionStateMachine
{
    public LocomotionPhase Phase { get; private set; } = LocomotionPhase.Grounded;
    public LocomotionSubPhase SubPhase { get; private set; } = LocomotionSubPhase.Idle;
    public float CoyoteTimer { get; private set; }
    public bool IsCoyoteActive => CoyoteTimer > 0f;
    private readonly float coyoteDuration;
    private readonly float verticalThreshold;
    public LocomotionStateMachine(float coyoteDuration, float verticalThreshold)
    {
        this.coyoteDuration = coyoteDuration;
        this.verticalThreshold = verticalThreshold;
    }
    public void Tick(bool isGrounded, float verticalVelocity, float horizontalSpeed, float deltaTime)
    {
        // Coyote
        if (isGrounded)
            CoyoteTimer = coyoteDuration;
        else
            CoyoteTimer -= deltaTime;

        // Phase + SubPhase
        if (isGrounded)
        {
            Phase = LocomotionPhase.Grounded;
            SubPhase = horizontalSpeed > 0.1f
                ? LocomotionSubPhase.Moving
                : LocomotionSubPhase.Idle;
        }
        else
        {
            Phase = LocomotionPhase.Airborne;
            if (verticalVelocity > verticalThreshold)
                SubPhase = LocomotionSubPhase.Rising;
            else if (verticalVelocity < -verticalThreshold)
                SubPhase = LocomotionSubPhase.Falling;
            else
                SubPhase = LocomotionSubPhase.Suspended;
        }
    }
    public void Reset()
    {
        Phase = LocomotionPhase.Grounded;
        SubPhase = LocomotionSubPhase.Idle;
        CoyoteTimer = 0f;
    }
}
