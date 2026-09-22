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
    private FighterAnimator fighterAnimator;
    private ActionSystem actionSystem;

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

    [Header("Nombres de Animaciones")]
    [SerializeField] private string fallAnimState = "Fall";
    [SerializeField] private string fallLandAnimState = "Fall_Land";

    private void Awake()
    {
        if (movement == null) movement = GetComponent<CharacterMovement>();
        if (fighterAnimator == null) fighterAnimator = GetComponent<FighterAnimator>();
        if (actionSystem == null) actionSystem = GetComponent<ActionSystem>();
    }

    private void Update()
    {
        UpdateGroundedAndCoyote();
        UpdateLocomotionPhases();

        if (actionSystem == null || !actionSystem.IsActive)
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

            if (fighterAnimator != null && fighterAnimator.IsActionPlaying) return;

            fighterAnimator.SetLocomotionBlend(blendValue);
        }
        else if (Phase == LocomotionPhase.Airborne)
        {
            if (SubPhase == LocomotionSubPhase.Falling)
            {
                if (fighterAnimator != null && fighterAnimator.IsActionPlaying) return;

                bool isCurrentlyFalling = fighterAnimator.Animator.GetCurrentAnimatorStateInfo(0).IsName(fallAnimState);
                bool isCurrentlyFallLand = fighterAnimator.Animator.GetCurrentAnimatorStateInfo(0).IsName(fallLandAnimState);
                bool isTransitioningToFall = fighterAnimator.Animator.IsInTransition(0) && fighterAnimator.Animator.GetNextAnimatorStateInfo(0).IsName(fallAnimState);
                bool isTransitioningToFallLand = fighterAnimator.Animator.IsInTransition(0) && fighterAnimator.Animator.GetNextAnimatorStateInfo(0).IsName(fallLandAnimState);

                if (isCurrentlyFalling || isTransitioningToFall)
                    return;

                if (isCurrentlyFallLand || isTransitioningToFallLand)
                    return;

                fighterAnimator.PlayAnimation(fallAnimState, 0.15f);
            }
        }
    }
}