using UnityEngine;

public abstract class BaseState
{
    protected PlayerMovement pm;

    public BaseState(PlayerMovement player)
    {
        this.pm = player;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void ExitState();
}

