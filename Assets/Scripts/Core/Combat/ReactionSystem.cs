using UnityEngine;

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

    private FighterEntity fighter;

    private float reactionTimer;
    private bool mirrorToggle;
    private static readonly int MirrorHitHash = Animator.StringToHash("MirrorHit");

    private void Awake()
    {
        fighter = GetComponent<FighterEntity>();
    }

    private void Update()
    {
        if (!IsReacting || CurrentReaction == ReactionState.Dead)
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

        fighter.Actions.InterruptAction();

        CurrentReaction = ReactionState.Hit;
        reactionTimer = Mathf.Max(0f, duration);

        fighter.Movement?.SetMovementLock(MovementLockSource.Reaction, true);
        fighter.Movement?.StopHorizontalMovement();

        mirrorToggle = !mirrorToggle;

        fighter.Visuals.PlayReaction("Hit");

        CloseHitboxes();
    }

    public void StartDeath()
    {
        if (CurrentReaction == ReactionState.Dead)
            return;

        fighter.Actions?.InterruptAction();
        CurrentReaction = ReactionState.Dead;

        fighter.Movement?.SetMovementLock(MovementLockSource.Reaction, true);
        fighter.Movement?.StopHorizontalMovement();

        fighter.Movement.Controller.enabled = false;

        fighter.Visuals.PlayReaction("Death", 0.1f);
    }

    public void EndReaction()
    {
        if (CurrentReaction == ReactionState.Dead) return;

        CurrentReaction = ReactionState.None;
        reactionTimer = 0f;

        fighter.Movement?.SetMovementLock(MovementLockSource.Reaction, false);
        mirrorToggle = false;
    }

    private void CloseHitboxes()
    { 
        if (fighter == null) return;

        for (int i = 0; i < 5; i++)
            fighter.AnimEvent_CloseHitbox(i);
    }
}
