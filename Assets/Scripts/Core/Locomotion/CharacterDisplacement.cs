using UnityEngine;

public class CharacterDisplacement : MonoBehaviour
{
    public bool IsDisplacing { get; private set; }

    private DisplacementData currentData;
    private Vector3 direction;
    private float elapsedTime;
    private Vector3 previousOffset;

    private Vector3 currentFrameDelta;
    private float currentFrameVerticalVelocity;

    public bool BlocksHorizontalMovement =>
        IsDisplacing &&
        currentData != null &&
        currentData.blockHorizontalMovement;

    public bool UsesGravity =>
        !IsDisplacing ||
        currentData == null ||
        currentData.applyGravityDuringDisplacement;

    public Vector3 CurrentFrameDelta => currentFrameDelta;

    public Vector3 CurrentFrameVerticalDelta =>
        new Vector3(0f, currentFrameDelta.y, 0f);

    public float CurrentFrameVerticalVelocity =>
        currentFrameVerticalVelocity;

    public bool StartDisplacement( DisplacementData data,Vector3 displacementDirection)
    {
        if (data == null)
        {
            Debug.LogWarning( $"{name}: Se intentó iniciar un desplazamiento sin DisplacementData.", this);
            return false;
        }

        if (data.displacementType != DisplacementType.Vertical)
        {
            displacementDirection.y = 0f;

            if (displacementDirection.sqrMagnitude < 0.0001f)
            {
                Debug.LogWarning( $"{name}: El desplazamiento necesita una dirección válida.",this);
                return false;
            }

            direction = displacementDirection.normalized;
        }
        else
            direction = Vector3.zero;

        currentData = data;
        elapsedTime = 0f;
        previousOffset = Vector3.zero;

        currentFrameDelta = Vector3.zero;
        currentFrameVerticalVelocity = 0f;

        IsDisplacing = true;

        return true;
    }

    public void StopDisplacement()
    {
        IsDisplacing = false;
        currentData = null;
        elapsedTime = 0f;
        previousOffset = Vector3.zero;
        direction = Vector3.zero;

        currentFrameDelta = Vector3.zero;
        currentFrameVerticalVelocity = 0f;
    }

    public Vector3 SimulateDisplacement(float deltaTime)
    {
        currentFrameDelta = Vector3.zero;
        currentFrameVerticalVelocity = 0f;

        if (!IsDisplacing || currentData == null)
            return Vector3.zero;

        elapsedTime += deltaTime;

        float normalizedTime = Mathf.Clamp01(elapsedTime / currentData.duration);

        Vector3 currentOffset = CalculateOffset(normalizedTime);
        currentFrameDelta = currentOffset - previousOffset;
        previousOffset = currentOffset;


        if (deltaTime > 0f)
            currentFrameVerticalVelocity = currentFrameDelta.y / deltaTime;

        if (normalizedTime >= 1f)
        {
            IsDisplacing = false;
            currentData = null;
        }

        return currentFrameDelta;
    }

    private Vector3 CalculateOffset( float normalizedTime)
    {
        float progress = currentData.motionCurve.Evaluate(normalizedTime);

        switch (currentData.displacementType)
        {
            case DisplacementType.Linear:
                return direction * currentData.distance * progress;

            case DisplacementType.Vertical:
                return Vector3.up * currentData.verticalDistance * progress;

            case DisplacementType.Parabolic:
                Vector3 horizontalOffset = direction * currentData.distanceForParabola * progress;
                float verticalProgress =  4f * normalizedTime * (1f - normalizedTime);
                Vector3 verticalOffset = Vector3.up * currentData.height * verticalProgress;

                return horizontalOffset + verticalOffset;
            
            case DisplacementType.Impulse:
                return Vector3.zero;

            default:
                return Vector3.zero;
        }
    }
}