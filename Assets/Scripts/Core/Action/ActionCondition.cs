using System;

[Serializable]
public abstract class ActionCondition
{
    public abstract bool IsMet(ActionContext context);
}

[Serializable]
public class GroundedCondition : ActionCondition
{
    public bool allowCoyoteTime = false;

    public override bool IsMet(ActionContext context)
    {
        if (context.isGrounded) return true;
        if (allowCoyoteTime && context.isCoyoteActive) return true;
        return false;
    }
}