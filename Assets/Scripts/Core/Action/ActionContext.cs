using UnityEngine;

public struct ActionContext
{
    public LocomotionPhase locomotionPhase;
    public LocomotionSubPhase locomotionSubPhase;

    public Vector3 movementDirection;

    public ActionData currentAction;

    public bool isBusy;
    public bool hasTarget;

    public bool isGrounded;
    public bool isCoyoteActive;

    public ActionContext(
        LocomotionSystem locomotion,
        Vector3 movementDirection,
        ActionData currentAction,
        bool hasTarget)
    {
        locomotionPhase = locomotion.Phase;
        locomotionSubPhase = locomotion.SubPhase;

        this.movementDirection = movementDirection;
        this.currentAction = currentAction;

        isBusy = currentAction != null;
        this.hasTarget = hasTarget;

        isGrounded = locomotion.IsGrounded;
        isCoyoteActive = locomotion.IsCoyoteActive;
    }
}