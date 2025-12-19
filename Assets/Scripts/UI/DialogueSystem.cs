using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public enum DialogueState
{
    None,
    DialogueTyping,
    DialogueLineFinished,
    ChoicePresenting,
    ChoiceFeedbackTyping,
    ChoiceFeedbackFinished,
    DialogueEnding,
    ChoiceEnding
}

public class DialogueSystem : MonoBehaviour
{
    [Serializable]
    public class DialoguePreData
    {
        public DialogueData preDialogueData;
        [Space]
        public UnityEvent onDialogueFinished;

    }

    [Serializable]
    public class QuestionPreData
    {
        public QuestionData preQuestionData;
        [Space]
        public UnityEvent onChoiceCorrectEnd;
        public UnityEvent onChoiceIncorrectEnd;
    }

    public static DialogueSystem instance;

    public List<DialoguePreData> preDialoguesDatas = new List<DialoguePreData>();
    public List<QuestionPreData> preQuestionsDatas = new List<QuestionPreData>();

    private DialoguePreData currentDialogue;
    private QuestionPreData currentQuestion;
    private UnityEvent currentChoiceEvent;

    private int currentLine;
    public float typingSpeed = 0.05f;
    public float wordFadeDuration = 0.2f;

    public DialogueState currentDialogueState = DialogueState.None;

    private List<GameObject> choiceButtons = new List<GameObject>();

    private int selectedChoiceIndex = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        UIInputHandler.OnScrollDialogueChoices += ScrollChoice;
        UIInputHandler.OnSkipDialoguePressed += HandleSkipDialogue;
        StandartButton.OnChoiceButtonHoverEnter += OnChoiceButtonHovered;
    }

    private void OnDisable()
    {
        UIInputHandler.OnScrollDialogueChoices -= ScrollChoice;
        UIInputHandler.OnSkipDialoguePressed -= HandleSkipDialogue;
        StandartButton.OnChoiceButtonHoverEnter -= OnChoiceButtonHovered;
    }

    public DialogueState GetCurrentDialogueState()
    {
        return currentDialogueState;
    }

    public void StartDialogue(int indexDialogueData)
    {
        if (currentDialogueState != DialogueState.None) return;

        int _indexTemp = 0;
        foreach (DialoguePreData value in preDialoguesDatas)
        {
            if (value.preDialogueData.DialogueID != indexDialogueData)
            {
                _indexTemp++;
            }
            else
            {
                break;
            }
        }

        UIManager.instance.GetDialogueText().text = string.Empty;
        currentDialogue = preDialoguesDatas[_indexTemp];
        currentLine = 0;

        UIManager.instance.ShowDialoguePanel(true);
        UIManager.instance.ShowChoicesPanel(false);
        GameManager.instance.MovingCamera(false);
        GameManager.instance.InitialGameEnd();

        currentDialogueState = DialogueState.DialogueTyping;
        StartCoroutine(ShowDialogueLine());
    }

    public void TryAdvance()
    {
        switch (currentDialogueState)
        {
            case DialogueState.DialogueTyping:
                StopAllCoroutines();
                UIManager.instance.GetDialogueText().text = currentDialogue.preDialogueData.dialogueLines[currentLine];
                UIManager.instance.ShowContinueIcon(true);
                currentDialogueState = DialogueState.DialogueLineFinished;
                break;

            case DialogueState.DialogueLineFinished:
                UIManager.instance.ShowContinueIcon(false);
                NextDialogueLine();
                break;


            case DialogueState.ChoicePresenting:
                ConfirmSelectedChoice(selectedChoiceIndex);
                break;

            case DialogueState.ChoiceFeedbackTyping:
                StopAllCoroutines();
                string feedbackToDisplay = (currentChoiceEvent == currentQuestion.onChoiceCorrectEnd) ?
                                           currentQuestion.preQuestionData.correctFeedback :
                                           currentQuestion.preQuestionData.incorrectFeedback;
                UIManager.instance.GetDialogueText().text = feedbackToDisplay;
                UIManager.instance.ShowContinueIcon(true);
                currentDialogueState = DialogueState.ChoiceFeedbackFinished;
                break;

            case DialogueState.ChoiceFeedbackFinished:
                Endchoice(currentChoiceEvent);
                break;

            default:
                break;
        }
    }
 
    IEnumerator ShowDialogueLine()
    {
        currentDialogueState = DialogueState.DialogueTyping;
        UIManager.instance.GetDialogueText().text = string.Empty;
        UIManager.instance.ShowContinueIcon(false);

        string line = currentDialogue.preDialogueData.dialogueLines[currentLine];

        foreach (char ch in line)
        {
            if (currentDialogueState != DialogueState.DialogueTyping)
            {
                UIManager.instance.GetDialogueText().text = line;
                break;
            }
            UIManager.instance.GetDialogueText().text += ch;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (currentDialogueState == DialogueState.DialogueTyping)
        {
            currentDialogueState = DialogueState.DialogueLineFinished;
            UIManager.instance.ShowContinueIcon(true);
        }
    }

    private void NextDialogueLine()
    {
        currentLine++;
        UIManager.instance.ShowContinueIcon(false);
        UIManager.instance.GetDialogueText().text = string.Empty;

        if (currentLine < currentDialogue.preDialogueData.dialogueLines.Count)
        {
            StartCoroutine(ShowDialogueLine());
        }
        else
        {
            UIManager.instance.ShowContinueIcon(false);
            currentDialogueState = DialogueState.DialogueEnding;
            EndDialogue();
        }
    }

    private void HandleSkipDialogue()
    {
        switch (currentDialogueState)
        {
            case DialogueState.DialogueTyping:
            case DialogueState.DialogueLineFinished:
            case DialogueState.ChoiceFeedbackTyping:
                EndDialogue();
                break;
        }
    }

    public void ShowChoices(int indexDataQuestion)
    {
        if (currentDialogueState != DialogueState.None) return;

        int _indexTemp = 0;
        foreach (QuestionPreData value in preQuestionsDatas)
        {
            if (value.preQuestionData.QuestionID != indexDataQuestion)
            {
                _indexTemp++;
            }
            else
            {
                break;
            }
        }

        currentQuestion = preQuestionsDatas[_indexTemp];
        UIManager.instance.GetDialogueText().text = string.Empty;
        UIManager.instance.ShowChoicesPanel(true);
        UIManager.instance.ShowDialoguePanel(false);
        UIManager.instance.GetQuestionText().text = currentQuestion.preQuestionData.questionText;

        ClearChoices();

        selectedChoiceIndex = 0;

        for (int i = 0; i < currentQuestion.preQuestionData.options.Length; i++)
        {
            GameObject _buttonTemp = Instantiate(UIManager.instance.GetChoiceButtonPrefab(), UIManager.instance.GetChoiceContainer());
            if (_buttonTemp != null)
            {
                int choiceIndex = i;
                _buttonTemp.GetComponent<StandartButton>().SetAnswer(currentQuestion.preQuestionData.options[choiceIndex], choiceIndex);
            }
            choiceButtons.Add(_buttonTemp);
        }

        UpdateSelectedChoiceUI();

        GameManager.instance.InitialGameEnd();
        GameManager.instance.MovingCamera(false);
        currentDialogueState = DialogueState.ChoicePresenting;
    }

    private void ClearChoices()
    {
        foreach (GameObject button in choiceButtons)
        {
            Destroy(button);
        }
        choiceButtons.Clear();
    }

    private void ConfirmSelectedChoice(int choiceIndex)
    {
        if (currentDialogueState == DialogueState.ChoicePresenting)
        {
            if (choiceIndex == currentQuestion.preQuestionData.correctAnswerIndex)
            {
                currentChoiceEvent = currentQuestion.onChoiceCorrectEnd;
                StartCoroutine(TypeFeedback(currentQuestion.preQuestionData.correctFeedback)); 
            }
            else
            {
                currentChoiceEvent = currentQuestion.onChoiceIncorrectEnd; 
                StartCoroutine(TypeFeedback(currentQuestion.preQuestionData.incorrectFeedback));
            }
        }
    }

    public void ScrollChoice(float scroll)
    {
        if (currentDialogueState == DialogueState.ChoicePresenting && choiceButtons.Count > 0)
        {
            int previousSelected = selectedChoiceIndex;

            if (scroll > 0) 
            {
                selectedChoiceIndex--;
                if (selectedChoiceIndex < 0)
                {
                    selectedChoiceIndex = choiceButtons.Count - 1;
                }
            }
            else if (scroll < 0)
            {
                selectedChoiceIndex++;
                if (selectedChoiceIndex >= choiceButtons.Count)
                {
                    selectedChoiceIndex = 0;
                }
            }

            if (selectedChoiceIndex != previousSelected)
            {
                UpdateSelectedChoiceUI();
            }
        }
    }

    private void UpdateSelectedChoiceUI()
    {
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            StandartButton choiceBtn = choiceButtons[i].GetComponent<StandartButton>();
            if (choiceBtn != null)
            {
                choiceBtn.SetSelected(i == selectedChoiceIndex);
            }
        }
    }

    private void OnChoiceButtonHovered(int index)
    {
        if (currentDialogueState == DialogueState.ChoicePresenting)
        {
            selectedChoiceIndex = index;
            UpdateSelectedChoiceUI();
        }
    }

    IEnumerator TypeFeedback(string feedbackText)
    {
        UIManager.instance.ShowDialoguePanel(true);
        UIManager.instance.ShowChoicesPanel(false);
        currentDialogueState = DialogueState.ChoiceFeedbackTyping;
        UIManager.instance.GetDialogueText().text = string.Empty;
        UIManager.instance.ShowContinueIcon(false);

        foreach (char ch in feedbackText)
        {
            if (currentDialogueState != DialogueState.ChoiceFeedbackTyping)
            {
                UIManager.instance.GetDialogueText().text = feedbackText;
                break;
            }
            UIManager.instance.GetDialogueText().text += ch;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (currentDialogueState == DialogueState.ChoiceFeedbackTyping)
        {
            currentDialogueState = DialogueState.ChoiceFeedbackFinished;
            UIManager.instance.ShowContinueIcon(true);
        }
    }

    public void EndDialogue()
    {
        currentDialogueState = DialogueState.DialogueEnding;
        StopAllCoroutines();
        currentLine = 0;
        UIManager.instance.GetDialogueText().text = string.Empty;

        UIManager.instance.ShowDialoguePanel(false);
        GameManager.instance.InitialGameStart();
        GameManager.instance.MovingCamera(true);
        UIManager.instance.ShowContinueIcon(false);
        currentDialogueState = DialogueState.None;
        currentDialogue.onDialogueFinished?.Invoke();
        currentDialogue = null;
    }

    public void Endchoice(UnityEvent currentEvent)
    {
        currentDialogueState = DialogueState.ChoiceEnding; 
        StopAllCoroutines();
        currentLine = 0;
        UIManager.instance.ShowDialoguePanel(false);

        UIManager.instance.GetDialogueText().text = string.Empty;
        GameManager.instance.InitialGameStart();
        GameManager.instance.MovingCamera(true);
        currentDialogueState = DialogueState.None;
        currentChoiceEvent?.Invoke();
        currentQuestion = null;
    }
}