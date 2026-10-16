using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public delegate void DelegatedGameStates();

    public DelegatedGameStates InitialGameStart;
    public DelegatedGameStates InitialGameEnd;

    public static GameManager instance;

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public Image fadeImage;
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;

    private Timer timer;

    public GameObject DefaultCam;
    private CinemachineInputAxisController AxisController;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (DefaultCam != null)
        {
            AxisController = DefaultCam.GetComponent<CinemachineInputAxisController>();
        }

        GamePrepate();
    }

    public void GamePrepate()
    {
        if (fadeImage != null)
        {
            fadeImage.color = fadeColor;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
        }

        StartFadeIn();

        timer = FindFirstObjectByType<Timer>();

        Invoke(nameof(GameStart), 0.2f);
    }

    public void GameStart()
    {
        InitialGameStart?.Invoke();
    }

    public void GameEnd()
    {
        InitialGameEnd?.Invoke();
    }

    public void MovingCamera(bool state)
    {
        if (AxisController == null)
            return;

        AxisController.enabled = state;
    }

    public void HidePanels()
    {
        if (UIManager.instance == null)
            return;

        UIManager.instance.interactablePanel?.SetActive(false);
        //UIManager.instance.hintPanel?.SetActive(false);
    }

    public Timer GetTimer()
    {
        return timer;
    }

    public void StartFadeOut(System.Action onComplete = null)
    {
        StartCoroutine(FadeOutCoroutine(onComplete));
    }

    public void StartFadeIn(System.Action onComplete = null)
    {
        StartCoroutine(FadeInCoroutine(onComplete));
    }

    private IEnumerator FadeOutCoroutine(System.Action onComplete)
    {
        if (fadeImage != null)
        {
            fadeImage.color = fadeColor;
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha =
                    Mathf.Lerp(0f, 1f, timer / fadeDuration);
            }

            yield return null;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
        }

        onComplete?.Invoke();
    }

    private IEnumerator FadeInCoroutine(System.Action onComplete)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha =
                    Mathf.Lerp(1f, 0f, timer / fadeDuration);
            }

            yield return null;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
        }

        onComplete?.Invoke();
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(FadeAndLoad(sceneIndex));

        Time.timeScale = 1f;

        UIManager.instance.ShowCursor(true);

        StartCoroutine(HideMouse());
    }

    public IEnumerator HideMouse()
    {
        yield return new WaitForSecondsRealtime(1f);

        UIManager.instance.ShowCursor(false);
    }

    public IEnumerator FadeAndLoad(int sceneIndex)
    {
        yield return StartCoroutine(FadeOutCoroutine(null));

        SceneManager.LoadScene(sceneIndex);
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void ResumeGame()
    {
        UIManager.instance.ShowPausePanel(false);
        Time.timeScale = 1f;
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void GamePause()
    {
        if (Time.timeScale == 1f)
        {
            UIManager.instance.ShowPausePanel(true);
            Time.timeScale = 0f;
        }
        else
        {
            UIManager.instance.ShowPausePanel(false);
            Time.timeScale = 1f;
        }
    }
}