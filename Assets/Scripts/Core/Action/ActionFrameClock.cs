using UnityEngine;

public class ActionFrameClock : MonoBehaviour
{
    [Header("Configuración de Frames")]
    [SerializeField] private int targetFPS = 60;
    [SerializeField] private bool manualAnimatorUpdate = false;

    private float timePerFrame;
    private float accumulator;

    public int CurrentFrame { get; private set; }
    public bool IsTicking { get; private set; } = false;
    public int TargetFPS { get => targetFPS; set => targetFPS = value; }

    // Delegado para ActionSystem
    public delegate void FrameTickHandler(int currentFrame);
    public event FrameTickHandler OnFrameTick;

    private void Awake()
    {
        SetTargetFPS(TargetFPS);
    }

    public void SetTargetFPS(int fps)
    {
        TargetFPS = Mathf.Max(1, fps);
        timePerFrame = 1f / TargetFPS;
    }

    public void StartClock()
    {
        CurrentFrame = 0;
        accumulator = 0f;
        IsTicking = true;
    }

    public void StopClock()
    {
        IsTicking = false;
        CurrentFrame = 0;
    }

    private void Update()
    {
        if (!IsTicking) return;

        accumulator += Time.deltaTime;

        while (accumulator >= timePerFrame)
        {
            CurrentFrame++;
            OnFrameTick?.Invoke(CurrentFrame);
            accumulator -= timePerFrame;
        }
    }

    public float FrameToSeconds(int frames) => frames * timePerFrame;
    public int SecondsToFrames(float seconds) => Mathf.RoundToInt(seconds * TargetFPS);
}