using UnityEngine;

public class CharacterFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterMovement movement;
    [SerializeField] private LocomotionSystem locomotion;
    [SerializeField] private ActionSimulationRunner actionSimulationRunner;

    [Header("Audio")]
    [SerializeField] private AudioSource characterSource;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Particles")]
    [SerializeField] private ParticleSystem landDust;
    [SerializeField] private ParticleSystem footstepDust;


    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<CharacterMovement>();

        if (locomotion == null)
            locomotion = GetComponent<LocomotionSystem>();

        if (actionSimulationRunner == null)
            actionSimulationRunner = GetComponent<ActionSimulationRunner>();
    }

    public void HandleLanding()
    {
        bool grounded = movement.IsGrounded;

        if (grounded)
        {
            if (landDust != null)
                landDust.Play();

            if (landSound != null || characterSource != null)
                characterSource.PlayOneShot(landSound);
        }
    }

    public void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        characterSource.PlayOneShot(footstepClips[index]);

        if (footstepDust != null)
            footstepDust.Play();
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySound(clip);
    }
}
