using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Game Elements")]
    public GameObject interactablePanel;

    public GameObject optionsPanel;
    public GameObject pausePanel;
    public GameObject InvetoryPanel;
    public GameObject creditsPanel;
    public GameObject hintPanel;
    public GameObject timerPanel;
    public TextMeshProUGUI hintText;

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

    public void AnimatePanelIn(GameObject panel)
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

    public void AnimatePanelOut(GameObject panel)
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
                interactablePanel.SetActive(false);
        }
        else
        {
            interactablePanel.SetActive(false);
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

    public void ShowTimerPanel(bool state)
    {
        if (state)
            AnimatePanelIn(timerPanel);
        else
            AnimatePanelOut(timerPanel);
    }

    public void ShowPausePanel(bool state)
    {
        if (state)
            AnimatePanelIn(pausePanel);
        else
            AnimatePanelOut(pausePanel);
    }

    public void ShowOptionsPanel(bool state)
    {
        if (state)
            AnimatePanelIn(optionsPanel);
        else
            AnimatePanelOut(optionsPanel);
    }

    public bool IsPanelActive()
    {
        return
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

    public void CleanInstance()
    {
        Destroy(instance);
    }
}

