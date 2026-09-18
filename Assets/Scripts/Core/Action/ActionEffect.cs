using System;
using UnityEngine;

[Serializable]
public abstract class ActionEffect
{
    public abstract void Execute(GameObject user, ActionContext context);
}

[Serializable]
public class DisplacementEffct : ActionEffect
{
    public enum DisplacementDirectionMode {
        Forward,            
        Backward,           
        Left,               
        Right,              
        InputDirection,     
        TargetRelative
    }

    public DisplacementData displacement;
    public DisplacementDirectionMode directionMode = DisplacementDirectionMode.Forward;

    public override void Execute(GameObject user, ActionContext context)
    {
        if (displacement == null) return;
        if (!user.TryGetComponent<CharacterMovement>(out var movement)) return;

        if (displacement.displacementType == DisplacementType.Impulse)
        {
            movement.SetVerticalVelocity(displacement.impulseForce);
            return;
        }

        Vector3 direction = CalculateDirection(user, context);
        movement.StartDisplacement(displacement, direction.normalized);
    }

    private Vector3 CalculateDirection(GameObject user, ActionContext context)
    {
        switch (directionMode)
        {
            case DisplacementDirectionMode.Backward:
                return -user.transform.forward;
            case DisplacementDirectionMode.Left:
                return -user.transform.right;
            case DisplacementDirectionMode.Right:
                return user.transform.right;
            case DisplacementDirectionMode.InputDirection:
                return context.movementDirection.sqrMagnitude > 0.01f
                    ? context.movementDirection
                    : user.transform.forward;
            case DisplacementDirectionMode.Forward:
            default:
                return user.transform.forward;
        }
    }
}


[Serializable]
public class DamageEffect : ActionEffect
{
    public AttackData attackData;
    public override void Execute(GameObject user, ActionContext context)
    {

    }
}

[Serializable]
public class AudioEffect : ActionEffect
{
    public AudioClip audioClip;
    public override void Execute(GameObject user, ActionContext context)
    {
        if (audioClip == null) return;
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySound(audioClip);
    }
}