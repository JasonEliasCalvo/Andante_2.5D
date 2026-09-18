using System;
using UnityEngine;

public enum InputCommandType
{
    Attack,
    Dash,
    Jump,
    Interact
}

[Flags]
public enum InputDirection
{
    None = 1 << 0, // 1
    Forward = 1 << 1, // 2
    Back = 1 << 2, // 4
    Left = 1 << 3, // 8
    Right = 1 << 4, // 16

    Any = None | Forward | Back | Left | Right
}

public struct InputCommand
{
    public InputCommandType type;
    public InputDirection direction;

    public Vector2 rawDirection;

    public float time;

    public InputCommand( InputCommandType type,InputDirection direction,
        Vector2 rawDirection,float time){
        this.type = type;
        this.direction = direction;
        this.rawDirection = rawDirection;
        this.time = time;
    }
}