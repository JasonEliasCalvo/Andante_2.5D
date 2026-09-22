using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class FighterAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayableActionSystem playableActionSystem;
    [SerializeField] private string locomotionBlendParam = "LocomotionBlend";
    [SerializeField] private float blendDampTime = 0.1f;

    private int locomotionBlendHash;
    private string currentStateName;
    public bool IsActionPlaying { get; private set; }
    public Animator Animator { get => animator; set => animator = value; }

    private void Awake()
    {
        if (Animator == null) Animator = GetComponentInChildren<Animator>(true);

        locomotionBlendHash = Animator.StringToHash(locomotionBlendParam);

        if (playableActionSystem == null)
            playableActionSystem = GetComponentInParent<PlayableActionSystem>();

        if (Animator != null) Animator.applyRootMotion = false;
    }

    public void Update()
    {
        if (!IsActionPlaying)
        {
            UpdateAnimationParams();
        }
    }
    
    public void UpdateAnimationParams()
    {
        if (playableActionSystem == null) return;

        LocomotionSystem locomotion = GetComponent<LocomotionSystem>();

        if (locomotion == null) return;

        Animator.SetBool("IsFalling", locomotion.SubPhase == LocomotionSubPhase.Falling);
        Animator.SetBool("IsGrounded", locomotion.IsGrounded);
    }
    public void SetLocomotionBlend(float blendValue)
    {
        if (IsActionPlaying || Animator == null) return;
        Animator.SetFloat(locomotionBlendHash, blendValue, blendDampTime, Time.deltaTime);
    }

    public void PlayAction(AnimationClip clip, int totalFrames, int targetFPS, float crossfadeDuration = 0.05f)
    {
        if (Animator == null || clip == null) return;

        Animator.speed = 1f;
        playableActionSystem?.PlayAction(clip, crossfadeDuration);
    }

    public void StopAction( float crossfadeDuration = 0.01f)
    {
        playableActionSystem?.StopAction(crossfadeDuration);
    }

    public void PlayAnimation(string stateName, float crossfadeDuration = 0.05f)
    {
        if (Animator == null || string.IsNullOrEmpty(stateName)) return;
        currentStateName = stateName;
        Animator.CrossFade(stateName, crossfadeDuration);
    }

    public void PlayReaction(string stateName, float crossfadeDuration = 0.05f)
    {
        if (Animator == null || string.IsNullOrEmpty(stateName)) return;
        currentStateName = stateName;
        Animator.CrossFade(stateName, crossfadeDuration);
    }

    public void PauseAnimatorForFrameControl()
    {
        if (Animator != null) Animator.speed = 0f;
    }

    public void ResumeAnimator()
    {
        if (Animator != null) Animator.speed = 1f;
    }

    public void SetActionPlaying(bool value)
    {
        IsActionPlaying = value;
        if (!value) ResumeAnimator();
    }
}