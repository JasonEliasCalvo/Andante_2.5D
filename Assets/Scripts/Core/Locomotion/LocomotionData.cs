using UnityEngine;

[CreateAssetMenu(
    fileName = "New Locomotion Data",
    menuName = "Character/Locomotion Data"
)]
public class LocomotionData : ScriptableObject
{
    [Header("Ground Movement")]
    [Min(0f)]
    public float walkSpeed = 5f;

    [Min(0f)]
    public float acceleration = 15f;

    [Min(0f)]
    public float deceleration = 20f;

    [Header("Air Movement")]
    [Min(0f)]
    public float airAcceleration = 10f;

    [Min(0f)]
    public float airDeceleration = 10f;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float fallGravityMultiplier = 1.8f;
    public float riseGravityMultiplier = 1f;

    [Min(0f)]
    public float terminalVelocity = 30f;

    [Header("Rotation")]
    [Min(0f)]
    public float rotationSmoothTime = 0.12f;

    [Header("Grounding")]
    public float groundedVerticalVelocity = -2f;
}

