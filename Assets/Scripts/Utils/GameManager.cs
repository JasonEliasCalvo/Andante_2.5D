using JetBrains.Annotations;
using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public delegate void DelegatedGameStates();
    public DelegatedGameStates eventGameStart;
    public DelegatedGameStates eventGameEnd;
    public DelegatedGameStates eventTypingGameStart;
    public DelegatedGameStates eventTypingGameReset;
    public DelegatedGameStates eventTypingGameEnd;
    public DelegatedGameStates eventSyllableGameStart;
    public DelegatedGameStates eventSyllableGameEnd;
    public DelegatedGameStates eventHappyGameStart;
    public DelegatedGameStates eventHappyGameEnd;
    public DelegatedGameStates eventMemoryGameStart;
    public DelegatedGameStates eventMemoryGameReset;
    public DelegatedGameStates eventMemoryGameEnd;
    public static GameManager instance;

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public Image fadeImage;
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;

    private Timer timer;
    [SerializeField] float initiateTime;

    public GameObject DefaultCam;
    private CinemachineInputAxisController AxisController;

    [SerializeField] private float checkInterval = 0.2f;
    [SerializeField] private float checkTimer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        AxisController = DefaultCam.GetComponent<CinemachineInputAxisController>();
        GamePrepate();
    }

    public void GamePrepate()
    {
        fadeImage.color = fadeColor;
        fadeCanvasGroup.alpha = 1f;
        StartFadeIn();

        timer = FindFirstObjectByType<Timer>();
        Invoke(nameof(InitialGameStart), 0.2f);
    }

    public void InitialGameStart()
    {
        eventGameStart?.Invoke();
    }

    public void InitialGameEnd()
    {
        eventGameEnd?.Invoke();
    }

    public void MovingCamera(bool state)
    {
        if (AxisController == null) return;

        AxisController.enabled = state;
    }

    private void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckInteractionZones();
        }
    }

    private void CheckInteractionZones()
    {
        var triggerZones = FindObjectsByType<TriggerZone>(FindObjectsSortMode.None);
        var droppableZones = FindObjectsByType<DroppableZone>(FindObjectsSortMode.None);
        var pl = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);


        bool anyPlayerNear = triggerZones.Any(tz => tz.IsPlayerNear() || tz.IsPlayerInTrigger()) ||
                             droppableZones.Any(dz => dz.IsPlayerNear() || dz.IsPlayerInTrigger());

        if (!anyPlayerNear && pl.Length > 0)
        {
            HidePanels();
            Debug.Log("No hay jugadores cerca de zonas interactivas.");
        }
    }

    public void HidePanels()
    {
        UIManager.instance.interactablePanel.SetActive(false);
        UIManager.instance.removeItemPanel.SetActive(false);
        UIManager.instance.hintPanel.SetActive(false);
    }

    public void TypingGameStart()
    {
        timer.eventEndTime += ResetTypingGame;
        timer.Initiate(initiateTime);
        eventTypingGameStart?.Invoke();
    }

    public void ResetTypingGame()
    {
        eventTypingGameReset?.Invoke();
    }

    public void TypingGameEnd()
    {
        timer.eventEndTime -= ResetTypingGame;
        eventTypingGameEnd?.Invoke();
    }

    public void MemoryGameStart()
    {
        timer.eventEndTime += ResetMemoryGame;
        timer.Initiate(90f);
        eventMemoryGameStart?.Invoke();
    }

    private void ResetMemoryGame()
    {
        eventMemoryGameReset?.Invoke();
    }

    private void EndMemoryGame()
    {
        timer.eventEndTime -= EndMemoryGame;
        eventMemoryGameReset?.Invoke();
    }

    public void SyllableGameStart()
    {
        eventSyllableGameStart?.Invoke();
    }

    public void SyllableGameEnd()
    {
        eventSyllableGameEnd?.Invoke();
    }

    public void HappyGameStart()
    {
        eventHappyGameStart?.Invoke();
    }

    public void HappyGameEnd()
    {
        eventHappyGameEnd?.Invoke();
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

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1;
        onComplete?.Invoke();
    }

    private IEnumerator FadeInCoroutine(System.Action onComplete)
    {
        Debug.Log("entro al fade");
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0;
        onComplete?.Invoke();
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(FadeAndLoad(sceneIndex));
        Time.timeScale = 1;
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
    public void TogglePause()
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
