using UnityEngine;

public enum LocomotionPhase
{
    Grounded,
    Airborne
}

public enum LocomotionSubPhase
{
    Idle,
    Moving,
    Rising,
    Falling,
    Suspended
}

[RequireComponent(typeof(CharacterMovement))]
public class LocomotionSystem : MonoBehaviour
{
    [Header("References")]
    private CharacterMovement movement;
    [SerializeField] private FighterEntity fighterEntity;
    private ActionSimulationRunner ActionSimulationRunner;

    [Header("Current Locomotion")]
    public LocomotionPhase Phase { get; private set; }
    public LocomotionSubPhase SubPhase { get; private set; }


    [Header("Runtime")]
    public bool IsGrounded { get; private set; }
    public bool IsCoyoteActive => coyoteTimer > 0f;
    public bool IsAirborne { get; private set; }

    public bool IsMoving { get; private set; }
    public bool IsRising { get; private set; }
    public bool IsFalling { get; private set; }

    [SerializeField]
    private float verticalThreshold = 3f;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteDuration = 0.15f;
    private float coyoteTimer;

    [Header("Animaciones")]
    [SerializeField] private string fallAnimState = "Fall";
    [SerializeField] private string fallLandAnimState = "Fall_Land";
    [SerializeField] private string locomotionBlendParam = "LocomotionBlend";
    [SerializeField] private float blendDampTime = 0.1f;

    private int locomotionBlendHash;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<CharacterMovement>();
        if (fighterEntity == null) fighterEntity = GetComponent<FighterEntity>();
        if (ActionSimulationRunner == null) ActionSimulationRunner = GetComponent<ActionSimulationRunner>();

        locomotionBlendHash = Animator.StringToHash(locomotionBlendParam);
    }

    private void Update()
    {
        UpdateGroundedAndCoyote();
        UpdateLocomotionPhases();

        if (ActionSimulationRunner != null)
        {
            UpdateLocomotionAnimations();
        }
    }

    private void UpdateGroundedAndCoyote()
    {
        IsGrounded = movement.IsGrounded;

        if (IsGrounded)
            coyoteTimer = coyoteDuration;
        else
            coyoteTimer -= Time.deltaTime;
    }

    private void UpdateLocomotionPhases()
    {
        float verticalVel = movement.EffectiveVerticalVelocity;
        float horizontalSpeed = movement.RuntimeData.desiredVelocity.magnitude;

        if (IsGrounded)
        {
            Phase = LocomotionPhase.Grounded;
            SubPhase = horizontalSpeed > 0.1f ? LocomotionSubPhase.Moving : LocomotionSubPhase.Idle;
        }
        else
        {
            Phase = LocomotionPhase.Airborne;

            if (verticalVel > verticalThreshold)
                SubPhase = LocomotionSubPhase.Rising;

            else if (verticalVel < -verticalThreshold && !movement.IsGrounded)
                SubPhase = LocomotionSubPhase.Falling;

            else 
                SubPhase = LocomotionSubPhase.Suspended;
        }
    }

    private void UpdateLocomotionAnimations()
    {
        if (Phase == LocomotionPhase.Grounded)
        {
            float speed = movement.RuntimeData.desiredVelocity.magnitude;
            float maxSpeed = movement.LocomotionData != null ? movement.LocomotionData.walkSpeed : 1f;
            float blendValue = Mathf.Clamp01(speed / maxSpeed);

            if (fighterEntity == null && fighterEntity.Visuals == null) return;
            
            fighterEntity.Visuals.SetLocomotionBlend(locomotionBlendHash, blendValue, blendDampTime);
        }
        else if (Phase == LocomotionPhase.Airborne)
        {
            if (SubPhase == LocomotionSubPhase.Falling)
            {
 
            }
        }
    }
}