public class ActionResolver
{
    public ActionData Resolve(InputCommand command, ActionContext context, MoveSet moveSet)
    {
        if (moveSet == null || moveSet.actions == null)
            return null;

        for (int i = 0; i < moveSet.actions.Count; i++)
        {
            ActionData action = moveSet.actions[i];

            if (action == null)
                continue;

            if (action.inputType != command.type)
                continue;

            if (!IsDirectionValid(action.allowedDirections, command.direction))
                continue;

            if (!IsContextValid(action, context))
                continue;

            return action;
        }

        return null;
    }
    private bool IsContextValid( ActionData action, ActionContext context)
    {
        for (int i = 0; i < action.conditions.Count; i++)
        {
            if (action.conditions[i] == null) continue;

            if (!action.conditions[i].IsMet(context))
                return false;
        }

        return true;
    }

    private bool IsDirectionValid(InputDirection allowedDirections, InputDirection inputDirection)
    {
        return (allowedDirections & inputDirection) != 0;
    }
}