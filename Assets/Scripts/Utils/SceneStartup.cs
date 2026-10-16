using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class SceneStartup : MonoBehaviour
{
    public enum StartupType
    {
        None,
        Video,
        Animation
    }

    [Header("Startup")]
    [SerializeField] private StartupType startupType;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoRoot;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animationState = "Intro";
    [SerializeField] private float animationDuration = 1f;

    public static bool startupFinished;

    private void Start()
    {
        switch (startupType)
        {
            case StartupType.None:
                StartGame();
                break;

            case StartupType.Video:
                StartVideo();
                break;

            case StartupType.Animation:
                StartCoroutine(StartAnimation());
                break;
        }
    }

    private void StartGame()
    {
        if (startupFinished)
            return;

        startupFinished = true;

        if (GameManager.instance == null)
        {
            Debug.LogError("No existe GameManager en esta escena.");
            return;
        }

        GameManager.instance.GamePrepare();
    }

    #region VIDEO

    private void StartVideo()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("SceneStartup: No hay VideoPlayer asignado.");
            StartGame();
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        videoPlayer.loopPointReached += OnVideoFinished;

        if (videoRoot != null)
        {
            videoRoot.SetActive(true);
        }

        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        player.loopPointReached -= OnVideoFinished;

        if (videoRoot != null)
        {
            videoRoot.SetActive(false);
        }

        StartGame();
    }

    #endregion

    #region ANIMATION

    private IEnumerator StartAnimation()
    {
        if (animator != null)
        {
            animator.Play(animationState, 0, 0f);
        }

        yield return new WaitForSecondsRealtime(animationDuration);

        StartGame();
    }

    #endregion

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}