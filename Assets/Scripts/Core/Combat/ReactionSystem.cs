using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class ReactionSystem : MonoBehaviour
{
    public enum ReactionState
    {
        None,
        Hit,
        Knockdown,
        Launch,
        Dead
    }

    [Header("Runtime")]
    public ReactionState CurrentReaction { get; private set; }

    public bool IsReacting => CurrentReaction != ReactionState.None;

    private CharacterMovement movement;
    private ActionSystem actionSystem;

    private FighterAnimator fighterAnimator;
    private Animator animator;

    private float reactionTimer;

    private bool mirrorToggle;

    private static readonly int MirrorHitHash = Animator.StringToHash("MirrorHit");

    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        actionSystem = GetComponent<ActionSystem>();

        fighterAnimator = GetComponent<FighterAnimator>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!IsReacting)
            return;

        if (CurrentReaction == ReactionState.Dead)
            return;

        reactionTimer -= Time.deltaTime;

        if (reactionTimer <= 0f)
        {
            EndReaction();
        }
    }

    public void StartHit(float duration)
    {
        if (CurrentReaction == ReactionState.Dead)
            return;

        if (actionSystem != null && actionSystem.IsActive)
        {
            actionSystem.InterruptAction();
        }

        CurrentReaction = ReactionState.Hit;
        reactionTimer = Mathf.Max(0f, duration);

        movement.SetMovementLock(
            MovementLockSource.Reaction,
            true
        );

        movement.StopHorizontalMovement();

        mirrorToggle = !mirrorToggle;

        if (animator != null)
        {
            animator.SetBool(MirrorHitHash, mirrorToggle);

            animator.Play("Hit", -1, 0f);
        }

        CloseHitboxes();
    }

    public void StartDeath()
    {
        if (CurrentReaction == ReactionState.Dead)
            return;

        if (actionSystem != null && actionSystem.IsActive)
        {
            actionSystem.InterruptAction();
        }

        CurrentReaction = ReactionState.Dead;

        movement.SetMovementLock(
            MovementLockSource.Reaction,
            true
        );

        movement.StopHorizontalMovement();

        if (movement.Controller != null)
        {
            movement.Controller.enabled = false;
        }

        animator?.CrossFade("Death", 0.1f);
    }

    public void EndReaction()
    {
        if (CurrentReaction == ReactionState.Dead)
            return;

        CurrentReaction =
            ReactionState.None;

        reactionTimer = 0f;

        movement.SetMovementLock(
            MovementLockSource.Reaction,
            false
        );

        mirrorToggle = false;
    }

    private void CloseHitboxes()
    {
        FighterEntity fighter =
            GetComponent<FighterEntity>();

        if (fighter == null)
            return;

        fighter.AnimEvent_CloseHitbox(0);
        fighter.AnimEvent_CloseHitbox(1);
        fighter.AnimEvent_CloseHitbox(2);
        fighter.AnimEvent_CloseHitbox(3);
        fighter.AnimEvent_CloseHitbox(4);
    }
}
