using System.Collections.Generic;
using UnityEngine;

public class InputBuffer
{
    private readonly Queue<InputCommand> commands = new();

    private readonly float bufferDuration;

    public InputBuffer(float bufferDuration = 0.25f)
    {
        this.bufferDuration = bufferDuration;
    }

    public void Add(InputCommand command)
    {
        commands.Enqueue(command);
    }

    public bool TryPeek(out InputCommand command)
    {
        RemoveExpiredCommands();

        if (commands.Count == 0)
        {
            command = default;
            return false;
        }

        command = commands.Peek();
        return true;
    }

    public bool TryConsume(out InputCommand command)
    {
        RemoveExpiredCommands();

        if (commands.Count == 0)
        {
            command = default;
            return false;
        }

        command = commands.Dequeue();
        return true;
    }

    public void Update()
    {
        RemoveExpiredCommands();
    }

    public void Clear()
    {
        commands.Clear();
    }

    public int Count
    {
        get
        {
            RemoveExpiredCommands();
            return commands.Count;
        }
    }

    private void RemoveExpiredCommands()
    {
        while (commands.Count > 0)
        {
            InputCommand command = commands.Peek();

            if (Time.time - command.time <= bufferDuration)
                break;

            commands.Dequeue();
        }
    }
}