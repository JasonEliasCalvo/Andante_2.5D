using UnityEngine;
public class DashState : BaseState
{
    public DashState(PlayerMovement pm) : base(pm) { }

    private Vector3 dashDirection;

    public override void EnterState()
    {
        Debug.Log("Entered Dash State");

        dashDirection = pm.GetMoveDirection();
        if (dashDirection == Vector3.zero)
            dashDirection = pm.transform.forward;

        pm.dashTimer = pm.dashDuration;
        pm.dashCooldownTimer = pm.dashCooldown;

        pm.animator?.SetTrigger("Dash");
    }

    public override void UpdateState()
    {
        pm.dashTimer -= Time.deltaTime;

        pm.horizontalVelocity = dashDirection * pm.dashSpeed;
        pm.verticalVelocity = 0f;

        if (pm.dashTimer > 0f)
            return;

        if (pm.controller.isGrounded)
        {
            if (pm.HasMovementInput)
                pm.ChangeState(pm.Walk);
            else
                pm.ChangeState(pm.Idle);
        }
        else
        {
            pm.ChangeState(pm.Fall);
        }
    }

    public override void FixedUpdateState()
    {

    }

    public override void ExitState()
    {
        pm.horizontalVelocity *= pm.dashForceStop;
    }
}
