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
    [SerializeField] private FighterEntity fighterEntity;

    [Header("Config")]
    [SerializeField] private float coyoteDuration = 0.15f;
    [SerializeField] private float verticalThreshold = 3f;

    [Header("Animation")]
    [SerializeField] private string locomotionBlendParam = "LocomotionBlend";
    [SerializeField] private float blendDampTime = 0.1f;
    private LocomotionStateMachine stateMachine;
    private int locomotionBlendHash;
    // ============================================================
    // PUBLIC API (proxy 1:1 hacia la state machine + movement)
    // ============================================================
    public LocomotionPhase Phase => stateMachine != null ? stateMachine.Phase : LocomotionPhase.Grounded;
    public LocomotionSubPhase SubPhase => stateMachine != null ? stateMachine.SubPhase : LocomotionSubPhase.Idle;
    public bool IsGrounded => fighterEntity.Movement != null && fighterEntity.Movement.IsGrounded;
    public bool IsCoyoteActive => stateMachine != null && stateMachine.IsCoyoteActive;
    public bool IsAirborne => Phase == LocomotionPhase.Airborne;
    public bool IsMoving => SubPhase == LocomotionSubPhase.Moving;
    public bool IsRising => SubPhase == LocomotionSubPhase.Rising;
    public bool IsFalling => SubPhase == LocomotionSubPhase.Falling;

    // LIFECYCLE

    private void Awake()
    {
        if (fighterEntity == null) fighterEntity = GetComponent<FighterEntity>();

        stateMachine = new LocomotionStateMachine(coyoteDuration, verticalThreshold);
        locomotionBlendHash = Animator.StringToHash(locomotionBlendParam);
    }

    private void Update()
    {
        if (fighterEntity.Movement == null || stateMachine == null) return;

        // 1. Tick de la FSM pura
        stateMachine.Tick(
            isGrounded: fighterEntity.Movement.IsGrounded,
            verticalVelocity: fighterEntity.Movement.EffectiveVerticalVelocity,
            horizontalSpeed: fighterEntity.Movement.RuntimeData.desiredVelocity.magnitude,
            deltaTime: Time.deltaTime);

        if (fighterEntity.Actions != null)
            UpdateLocomotionAnimation();
    }
  
    private void UpdateLocomotionAnimation()
    {
        if (fighterEntity == null || fighterEntity.Visuals == null) return;

        if (Phase == LocomotionPhase.Grounded)
        {
            float speed = fighterEntity.Movement.RuntimeData.desiredVelocity.magnitude;
            float maxSpeed = fighterEntity.Movement.LocomotionData != null
                 ? fighterEntity.Movement.LocomotionData.walkSpeed
                 : 1f;
            float blendValue = Mathf.Clamp01(speed / maxSpeed);

            fighterEntity.Visuals.SetLocomotionBlend(locomotionBlendHash, blendValue, blendDampTime);
        }

        else if (Phase == LocomotionPhase.Airborne)
        {
        }
    }
}