using UnityEngine;

[System.Serializable]
public struct MovementRuntimeData
{
    [Header("Velocity")]
    public Vector3 velocity;
    public float gravitytVerticalVelocity;
    public float efectiveVerticalVelocity;

    [Header("Desired Movement")]
    public Vector3 desiredVelocity;

    [Header("Rotation")]
    public float rotationVelocity;

    [Header("State")]
    public bool isGrounded;
}
