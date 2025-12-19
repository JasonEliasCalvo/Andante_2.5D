using UnityEngine;

public class AirborneState : BaseState
{
    public AirborneState(PlayerMovement pm) : base(pm) { }

    public override void EnterState()
    {
        Debug.Log("Entered Airborne State");
        pm.animator?.SetBool("IsGround", false);
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

        if (pm.controller.isGrounded)
        {
            if (pm.HasMovementInput)
                pm.ChangeState(pm.Walk);
            else
                pm.ChangeState(pm.Idle);
        }
    }

    public override void ExitState()
    {
        pm.animator?.SetBool("IsGround", true);
    }
}
