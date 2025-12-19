using UnityEngine;

public class JumpState : BaseState
{
    public JumpState(PlayerMovement pm) : base(pm) { }

    public override void EnterState()
    {
        Debug.Log("Entered Jump State");

        pm.verticalVelocity = pm.jumpForce;
        pm.coyoteCounter = 0f;

        pm.animator?.SetTrigger("Jump");
    }

    public override void FixedUpdateState()
    {
      
    }

    public override void UpdateState()
    {
        Vector3 moveDir = pm.GetMoveDirection();

        pm.desiredMoveDir = moveDir;
        pm.currentTargetSpeed = pm.walkSpeed;

        // cuando ya empieza a caer, pasamos a Fall
        if (pm.verticalVelocity <= 0f)
            pm.ChangeState(pm.Fall);
    }

    public override void ExitState()
    {
       
    }
}
