using UnityEngine;

public class WalkState : BaseState
{
    public WalkState(PlayerMovement pm) : base(pm) { }

    public override void EnterState()
    {
        Debug.Log("Entered Walk State");
    }

    public override void FixedUpdateState()
    {
    }

    public override void UpdateState()
    {
        Vector3 moveDir = pm.GetMoveDirection();

        pm.desiredMoveDir = moveDir;
        pm.currentTargetSpeed = pm.walkSpeed;

        pm.RotateCharacter(moveDir);
        pm.animator?.SetFloat("Speed", pm.horizontalVelocity.magnitude);

        if (!pm.HasMovementInput)
            pm.ChangeState(pm.Idle);

        if (pm.HasBufferedJump && pm.coyoteCounter > 0f)
        {
            pm.ConsumeJumpBuffer();
            pm.ChangeState(pm.Jump);
        }

        if (pm.coyoteCounter <= 0 && pm.verticalVelocity <= 0)
            pm.ChangeState(pm.Fall);

        if (pm.dashPressed && pm.CanDash())
            pm.ChangeState(pm.Dash);
    }

    public override void ExitState()
    {
       
    }
}
