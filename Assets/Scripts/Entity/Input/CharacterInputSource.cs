using UnityEngine;

public abstract class CharacterInputSource : MonoBehaviour
{
    public abstract Vector3 GetMovementDirection();

    public virtual bool TryConsumeCommand(out InputCommand command)
    {
        command = default;
        return false;
    }
}
