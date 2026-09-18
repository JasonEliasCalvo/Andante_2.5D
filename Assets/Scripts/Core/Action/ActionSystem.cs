using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class ActionSystem : MonoBehaviour
{
    [Header("References")]
    private CharacterMovement movement;
    private LocomotionSystem locomotion;
    private FighterAnimator fighterAnimator;
    private FighterEntity fighter;

    [Header("Runtime")]
    public ActionData CurrentAction { get; private set; }

    public bool IsActive => CurrentAction != null;

    private float actionTimer;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<CharacterMovement>();
        if (locomotion == null) locomotion = GetComponent<LocomotionSystem>();
        if (fighterAnimator == null) fighterAnimator = GetComponent<FighterAnimator>();
        if (fighter == null) fighter = GetComponent<FighterEntity>();
    }

    private void Update()
    {
        if (!IsActive)
            return;

        UpdateAction(Time.deltaTime);
    }

    public bool StartAction(ActionData action, ActionContext context)
    {
        if (action == null)
        {
            Debug.LogWarning($"{name}: Se intentó ejecutar una acción null.", this);
            return false;
        }

        if (IsActive)
            InterruptAction();

        CurrentAction = action;
        actionTimer = 0f;

        Debug.Log( $"{name} → ACTION → {action.actionName}");

        movement.SetMovementLock(MovementLockSource.Action, action.lockHorizontalMovement);

        for (int i = 0; i < action.effects.Count; i++)
        {
            if (action.effects[i] != null)
                action.effects[i].Execute(gameObject, context);
        }

        if (action.animationStateName != null && action.animationStateName != "")
            fighterAnimator?.PlayAction(action.animationStateName);

        fighterAnimator?.SetActionPlaying(true);
        return true;
    }

    private void UpdateAction(float deltaTime)
    {
        actionTimer += deltaTime;
        if (actionTimer >= GetActionDuration())
        {
            EndAction();
        }
    }

    private float GetActionDuration()
    {
        if (CurrentAction == null) return 0f;
        return CurrentAction.duration > 0f ? CurrentAction.duration : 0.5f;
    }

    public void EndAction()
    {
        if (!IsActive)
            return;

        Debug.Log($"{name} → ACTION END → {CurrentAction.actionName}");

        CleanupAction();
    }

    public void InterruptAction()
    {
        if (!IsActive)
            return;

        Debug.Log(
            $"{name} → ACTION INTERRUPTED → {CurrentAction.actionName}"
        );

        CleanupAction();

        CurrentAction = null;
        actionTimer = 0f;
    }

    private void CleanupAction()
    {
        movement.SetMovementLock(MovementLockSource.Action, false);
        movement.StopDisplacement();
        CloseHitboxes();

        CurrentAction = null;
        actionTimer = 0f;

        if (fighter != null && fighter.FighterAnimator != null)
        {
            fighter.FighterAnimator.SetActionPlaying(false);
        }
    }

    private void CloseHitboxes()
    {
        if (fighter == null)
            return;

        fighter.AnimEvent_CloseHitbox(0);
        fighter.AnimEvent_CloseHitbox(1);
        fighter.AnimEvent_CloseHitbox(2);
        fighter.AnimEvent_CloseHitbox(3);
        fighter.AnimEvent_CloseHitbox(4);
    }
}
