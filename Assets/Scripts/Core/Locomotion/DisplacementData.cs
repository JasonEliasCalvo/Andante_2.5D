using UnityEngine;

public enum DisplacementType{ Linear, Vertical, Parabolic, Impulse}

[CreateAssetMenu( fileName = "New Displacement", menuName = "Action/Movement/Displacement Data")]
public class DisplacementData : ScriptableObject
{
    [Header("Type")]
    public DisplacementType displacementType = DisplacementType.Linear;

    [Header("Horizontal")]
    public float distance = 2f;

    [Header("Vertical")]
    public float verticalDistance = 2f;

    [Header("Parabolic")]
    public float height = 2f;
    public float distanceForParabola = 2f;

    [Header("Impulse")]
    public float impulseForce = 10f;

    [Header("CinematicDisplacement")]
    [Min(0.01f)]
    public float duration = 0.25f;
    public AnimationCurve motionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Behaviour")]
    public bool blockHorizontalMovement = true;
    public bool applyGravityDuringDisplacement = false;

    public Vector3 CalculateOffset(Vector3 direction, float normalizedTime)
    {
        float progress = motionCurve.Evaluate(normalizedTime);
        switch (displacementType)
        {
            case DisplacementType.Linear:
                return direction * distance * progress;
            case DisplacementType.Vertical:
                return Vector3.up * verticalDistance * progress;
            case DisplacementType.Parabolic:
                Vector3 horizontalOffset = direction * distanceForParabola * progress;
                float verticalProgress = 4f * normalizedTime * (1f - normalizedTime);
                Vector3 verticalOffset = Vector3.up * height * verticalProgress;
                return horizontalOffset + verticalOffset;
            case DisplacementType.Impulse:
                return Vector3.zero;
            default:
                return Vector3.zero;
        }
    }
}