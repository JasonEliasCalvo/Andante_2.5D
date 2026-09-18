using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class FighterAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string locomotionBlendParam = "LocomotionBlend";
    [SerializeField] private float blendDampTime = 0.1f;

    private int locomotionBlendHash;
    private string currentStateName;
    public bool IsActionPlaying { get; private set; }

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>(true);

        locomotionBlendHash = Animator.StringToHash(locomotionBlendParam);
        if (animator != null) animator.applyRootMotion = false;
    }

    public void Update()
    {
        UpdateAnimationParams();
    }
    
    public void UpdateAnimationParams()
    {
        animator.SetBool("IsFalling", GetComponent<LocomotionSystem>().SubPhase == LocomotionSubPhase.Falling);
        animator.SetBool("IsGrounded", GetComponent<LocomotionSystem>().IsGrounded);
    }
    public void SetLocomotionBlend(float blendValue)
    {
        if (IsActionPlaying || animator == null) return;
        animator.SetFloat(locomotionBlendHash, blendValue, blendDampTime, Time.deltaTime);
    }

    public void PlayAction(string stateName, float crossfadeDuration = 0.05f)
    {
        if (animator == null || string.IsNullOrEmpty(stateName)) return;

        currentStateName = stateName;
        animator.CrossFade(stateName, crossfadeDuration);
    }

    public void SetActionPlaying(bool value)
    {
        IsActionPlaying = value;
    }
}