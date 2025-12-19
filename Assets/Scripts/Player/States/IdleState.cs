using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(PlayerMovement pm) : base(pm) { }

    public override void EnterState()
    {
        Debug.Log("Entered Idle State");
        pm.animator?.SetFloat("Speed", 0f);
    }

    public override void UpdateState()
    {
        if (pm.inputRaw.sqrMagnitude > 0.01f)
            pm.ChangeState(pm.Walk);

        if (pm.dashPressed && pm.CanDash())
            pm.ChangeState(pm.Dash);

        if (pm.HasBufferedJump && pm.coyoteCounter > 0f)
        {
            pm.ConsumeJumpBuffer();
            pm.ChangeState(pm.Jump);
        }

        if (!pm.controller.isGrounded && pm.verticalVelocity <= 0)
            pm.ChangeState(pm.Fall);
    }

    public override void FixedUpdateState() 
    {
       
    }

    public override void ExitState()
    {
        
    }
}
