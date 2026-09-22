using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActionFrameClock))]
public class ActionSystem : MonoBehaviour
{
    [Header("References")]
    private CharacterMovement movement;
    private FighterAnimator fighterAnimator;
    private ActionFrameClock frameClock;

    [Header("Runtime")]
    public ActionData CurrentAction { get; private set; }
    public ActionContext CurrentContext { get; private set; }
    public int CurrentFrame { get; private set; }
    public bool IsActive => CurrentAction != null;

    private List<ActionTimelineEntry> activeEntries = new List<ActionTimelineEntry>();

    private void Awake()
    {
        if (movement == null) movement = GetComponent<CharacterMovement>();
        if (fighterAnimator == null) fighterAnimator = GetComponent<FighterAnimator>();
        if (frameClock == null) frameClock = GetComponent<ActionFrameClock>();

        frameClock.OnFrameTick += HandleFrameTick;
    }

    private void OnDestroy()
    {
        if (frameClock != null)
            frameClock.OnFrameTick -= HandleFrameTick;
    }

    public bool StartAction(ActionData action, ActionContext context)
    {
        if (action == null) return false;

        if (IsActive)
        {
            if (!CanInterruptCurrentAction(action)) return false;
            InterruptAction();
        }

        CurrentAction = action;
        CurrentContext = context;
        CurrentFrame = 0;
        activeEntries.Clear();

        Debug.Log($"<color=cyan>{name} → START ACTION ({action.actionName}) | Priority: {action.priority}</color>");

        movement?.SetMovementLock(MovementLockSource.Action, action.lockHorizontalMovement);

        fighterAnimator?.PlayAction(
            action.animation,
            action.totalFrames,
            frameClock.TargetFPS,
            0f
        );

        fighterAnimator?.SetActionPlaying(true);
        frameClock.StartClock();
        HandleFrameTick(0);


        return true;
    }


    private void HandleFrameTick(int frame)
    {
        if (!IsActive) return;

        CurrentFrame = frame;

        for (int i = 0; i < CurrentAction.timeline.Count; i++)
        {
            var entry = CurrentAction.timeline[i];

            if (CurrentFrame == entry.startFrame)
            {
                entry.effect?.OnStart(gameObject, CurrentContext);
                activeEntries.Add(entry);
            }

            if (CurrentFrame >= entry.startFrame && CurrentFrame < entry.EndFrame)
            {
                int activeFrame = CurrentFrame - entry.startFrame;
                entry.effect?.OnUpdate(gameObject, CurrentContext, activeFrame);
            }
        }

        for (int i = activeEntries.Count - 1; i >= 0; i--)
        {
            var entry = activeEntries[i];
            if (CurrentFrame >= entry.EndFrame)
            {
                entry.effect?.OnEnd(gameObject, CurrentContext);
                activeEntries.RemoveAt(i);
            }
        }

        if (CurrentFrame >= CurrentAction.totalFrames)
        {
            EndAction();
        }
    }

    public void EndAction() => CleanupAction();
    public void InterruptAction() => CleanupAction();

    private void CleanupAction()
    {
        frameClock.StopClock();

        for (int i = 0; i < activeEntries.Count; i++)
        {
            activeEntries[i].effect?.OnEnd(gameObject, CurrentContext);
        }
        activeEntries.Clear();

        fighterAnimator?.StopAction(0.1f);

        movement?.SetMovementLock(MovementLockSource.Action, false);
        fighterAnimator?.SetActionPlaying(false);

        CurrentAction = null;
        CurrentContext = default;
        CurrentFrame = 0;
    }

    private bool CanInterruptCurrentAction(ActionData newAction)
    {
        if (CurrentAction == null) return true;
        if (newAction.priority > CurrentAction.priority) return true;
        if (CurrentAction.canBeCanceledOnHit && CurrentFrame >= CurrentAction.cancelStartFrame) return true;

        return false;
    }
}
