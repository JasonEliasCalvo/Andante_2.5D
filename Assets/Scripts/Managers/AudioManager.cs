
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    private AudioMixerGroup masterGroup;
    [SerializeField]
    public AudioSource sfx, music;



    [Header("Sonidos del Puzzle de Estatuas")]
    [SerializeField] private AudioClip statueFixClip;
    [SerializeField] private AudioClip puzzleCompleteClip;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        sfx.PlayOneShot(clip);
    }

    public void PlayStatueFixSound()
    {

        if (statueFixClip != null)
        {
            PlaySound(statueFixClip);
        }
    }

    public void PlayPuzzleCompleteSound()
    {
        if (puzzleCompleteClip != null)
        {
            PlaySound(puzzleCompleteClip);
        }
    }
    public void PlayLoopSound(AudioClip sound)
    {
        sfx.clip = sound;
        sfx.loop = true;
        sfx.Play();
    }
    public void StopLoopSound()
    {
        sfx.Stop();
        sfx.clip = null;
        sfx.loop = false;
    }

}