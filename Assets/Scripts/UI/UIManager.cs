using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Dialogue Elements")]
    public GameObject dialoguePanel;
    public GameObject choicesPanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI choicesText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private GameObject _continueIcon;

    [Header("UI Game Elements")]
    public GameObject interactablePanel;

    public GameObject optionsPanel;
    public GameObject pausePanel;
    public GameObject InvetoryPanel;
    public GameObject creditsPanel;
    public GameObject removeItemPanel;
    public GameObject hintPanel;
    public TextMeshProUGUI hintText;
    public GameObject missionPanel;
    public TextMeshProUGUI missionText;
    public GameObject skipPanel;


    [Header("UI Tutorial Elements")]
    public GameObject TutorialPanel;
    public TextMeshProUGUI titleText;
    public VideoPlayer tutorialVideo;
    public TextMeshProUGUI instructionText;
    public TutorialDatabase tutorialDatabase;

    [Header("UI Sounds")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip changeCharacterSound;
    public AudioClip victorySound;
    public AudioClip dropSound;

    private bool isCursorForcedVisible = false;

#if UNITY_EDITOR
    void OnEnable()
    {
        PlayerPrefs.DeleteAll();
    }
#endif

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (InvetoryPanel != null)
            ShowInventoryPanel();
    }

    private void AnimatePanelIn(GameObject panel)
    {
        if (panel.activeSelf) return;

        panel.SetActive(true);
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (DOTween.IsTweening(rt)) return;
        
        rt.localScale = Vector3.zero;

        rt.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    private void AnimatePanelOut(GameObject panel)
    {
        if (!panel.activeSelf) return;
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (DOTween.IsTweening(rt)) return;

        rt.DOScale(Vector3.zero, 0.25f)
          .SetEase(Ease.InBack)
          .SetUpdate(true)
          .OnComplete(() =>
          {
              panel.transform.localScale = Vector3.zero;
              panel.SetActive(false);
          });
    }

    public void ShowInteractablePanel(bool state)
    {
        if (!IsPanelActive())
        {
            if (state)
                AnimatePanelIn(interactablePanel);
            else
                AnimatePanelOut(interactablePanel);
        }
        else
        {
            AnimatePanelOut(interactablePanel);
        }
    }

    public void ShowSkipPanel(bool state)
    {
        if (state)
            AnimatePanelIn(skipPanel);
        else
            AnimatePanelOut(skipPanel);
    }

    public void ShowRemoveItemPanel(bool state)
    {
        if (state == removeItemPanel.activeSelf) return;

        if (!IsPanelActive())
        {
            if (state)
                AnimatePanelIn(removeItemPanel);
            else
                AnimatePanelOut(removeItemPanel);
        }
        else
        {
            AnimatePanelOut(removeItemPanel);
        }
    }


    public void ShowHintPanel(bool state, string message = "")
    {
        if (state == hintPanel.activeSelf) return;

        if (!IsPanelActive())
        {
            if (state)
            {
                AnimatePanelIn(hintPanel);
                hintText.text = message;
            }
            else
            {
                AnimatePanelOut(hintPanel);
                hintText.text = string.Empty;
            }
        }
        else
        {
            AnimatePanelOut(hintPanel);
            hintText.text = string.Empty;
        }

    }

    public void ShowPausePanel(bool state)
    {
        if (state)
            AnimatePanelIn(pausePanel);
        else
            AnimatePanelOut(pausePanel);
    }

    public void ShowDialoguePanel(bool state)
    {
        dialoguePanel.SetActive(state);
    }

    public void ShowChoicesPanel(bool state)
    {
        choicesPanel.SetActive(state);
    }

    public void ShowCreditsPanel(bool state)
    {
        creditsPanel.SetActive(state);
    }

    public void ShowOptionsPanel(bool state)
    {
        if (state)
            AnimatePanelIn(optionsPanel);
        else
            AnimatePanelOut(optionsPanel);
    }

    public void ShowMissionPanel(bool state)
    {
        if (state)
            AnimatePanelIn(missionPanel);
        else
            AnimatePanelOut(missionPanel);
    }

    public void UpdateMissionText(string newMission)
    {
        if (missionText == null) return;

        missionText.text = "";
        missionText.text = newMission;
    }


    public bool IsPanelActive()
    {
        return
       (dialoguePanel != null && dialoguePanel.activeSelf) ||
       (choicesPanel != null && choicesPanel.activeSelf) ||
       (TutorialPanel != null && TutorialPanel.activeSelf) ||
       (pausePanel != null && pausePanel.activeSelf) ||
       (optionsPanel != null && optionsPanel.activeSelf);
    }

    public bool IsMouseNecesary()
    {
        return IsPanelActive() || isCursorForcedVisible;
    }

    public void ShowCursor(bool state)
    {
        isCursorForcedVisible = state;
    }

    public void ShowInventoryPanel()
    {
        if (IsMouseNecesary())
        {
            InvetoryPanel.SetActive(false);
        }
        else
        {
            InvetoryPanel.SetActive(true);
        }
    }

    public void ShowTutorial(int tutorialID)
    {
        TutorialInfo info = tutorialDatabase.GetTutorialByID(tutorialID);

        GameManager.instance.MovingCamera(false);
        GameManager.instance.GameEnd();

        titleText.text = info.title;     
        tutorialVideo.clip = info.tutorialVideo;
        instructionText.text = info.instruction;
        AnimatePanelIn(TutorialPanel);
        tutorialVideo.Play();
        info = null;
    }

    public void HideTutorial()
    {
        GameManager.instance.MovingCamera(true);
        GameManager.instance.GameStart();
        AnimatePanelOut(TutorialPanel);
    }

    public void CleanInstance()
    {
        Destroy(instance);
    }
    public TextMeshProUGUI GetDialogueText() => dialogueText;
    public TextMeshProUGUI GetQuestionText() => choicesText;
    public Transform GetChoiceContainer() => choicesContainer;
    public GameObject GetChoiceButtonPrefab() => choiceButtonPrefab;
    public void ShowContinueIcon(bool state) => _continueIcon.SetActive(state);
}

