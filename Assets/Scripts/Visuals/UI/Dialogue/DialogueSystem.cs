using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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
    public static DialogueSystem instance;

    [Header("Dialogue Database")]
    [SerializeField] private DialogueDatabase dialoguesDatabase;

    [Header("Question Database")]
    [SerializeField] private QuestionDatabase questionsDatabase;

    [Header("Dialogue Events")]
    [SerializeField] private List<DialogueEventEntry> dialogueEvents = new();

    [Header("Question Events")]
    [SerializeField] private List<QuestionEventEntry> questionEvents = new();

    private DialogueData currentDialogue;
    private QuestionData currentQuestion;
    private UnityEvent currentChoiceEvent;

    private int currentLine;
    public float typingSpeed = 0.05f;
    public float wordFadeDuration = 0.2f;

    public DialogueState currentDialogueState = DialogueState.None;

    private readonly List<GameObject> choiceButtons = new();
    private int selectedChoiceIndex;
    private bool lastChoiceWasCorrect;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject continueIcon;

    [Header("Choice UI")]
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceContainer;

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

    public void StartDialogue(int dialogueID)
    {
        if (currentDialogueState != DialogueState.None)
            return;

        currentDialogue = FindDialogue(dialogueID);

        if (currentDialogue == null)
        {
            Debug.LogError(
                $"DialogueSystem: No existe un diálogo con ID {dialogueID}."
            );

            return;
        }

        StopAllCoroutines();

        currentLine = 0;

        ShowDialoguePanel(true);
        ShowChoicesPanel(false);
        ShowContinueIcon(false);

        ClearChoices();

        GameManager.instance.InitialGameEnd();
        GameManager.instance.MovingCamera(false);

        currentDialogueState = DialogueState.DialogueTyping;

        StartCoroutine(ShowDialogueLine());
    }

    private DialogueData FindDialogue(int dialogueID)
    {
        if (dialoguesDatabase == null)
        {
            Debug.LogError(
                "DialogueSystem: No hay DialogueDatabase asignado."
            );

            return null;
        }

        return dialoguesDatabase.dialogues.Find(dialogue =>
            dialogue != null &&
            dialogue.DialogueID == dialogueID
        );
    }

    public void TryAdvance()
    {
        switch (currentDialogueState)
        {
            case DialogueState.DialogueTyping:
                CompleteDialogueLine();
                break;

            case DialogueState.DialogueLineFinished:
                ShowContinueIcon(false);
                NextDialogueLine();
                break;

            case DialogueState.ChoicePresenting:
                ConfirmSelectedChoice(selectedChoiceIndex);
                break;

            case DialogueState.ChoiceFeedbackTyping:
                CompleteChoiceFeedback();
                break;

            case DialogueState.ChoiceFeedbackFinished:
                EndChoice();

                break;
            default:
                break;
        }
    }

    private void CompleteDialogueLine()
    {
        if (currentDialogue == null)
            return;

        StopAllCoroutines();
        dialogueText.text =
            currentDialogue.dialogueLines[currentLine].text;

        ShowContinueIcon(true);

        currentDialogueState =
            DialogueState.DialogueLineFinished;
    }

    private IEnumerator ShowDialogueLine()
    {
        if (currentDialogue == null) yield break;

        if (currentLine < 0 || currentLine >= currentDialogue.dialogueLines.Count)
        {
            EndDialogue();
            yield break;
        }
        currentDialogueState = DialogueState.DialogueTyping;
        dialogueText.text = string.Empty;
        ShowContinueIcon(false);

        string line = currentDialogue.dialogueLines[currentLine].text;

        foreach (char ch in line)
        {
            if (currentDialogueState != DialogueState.DialogueTyping)
            {
                dialogueText.text = line;
                break;
            }
            dialogueText.text += ch;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (currentDialogueState == DialogueState.DialogueTyping)
        {
            currentDialogueState = DialogueState.DialogueLineFinished;
            ShowContinueIcon(true);
        }
    }

    private void NextDialogueLine()
    {
        if (currentDialogue == null) return;

        currentLine++;
        ShowContinueIcon(false);
        dialogueText.text = string.Empty;

        if (currentLine < currentDialogue.dialogueLines.Count)
            StartCoroutine(ShowDialogueLine());
        else
            EndDialogue();
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

    public void ShowChoices(int questionID)
    {
        if (currentDialogueState != DialogueState.None) return;

        currentQuestion = FindQuestion(questionID);
        if (currentQuestion == null)
        {
            Debug.LogError(
                $"DialogueSystem: QuestionDatabase no contiene una pregunta con ID {questionID}."
            );

            return;
        }

        Debug.Log(
            $"DialogueSystem: Mostrando pregunta {questionID} con " +
            $"{currentQuestion.options.Length} opciones."
        );


        StopAllCoroutines();

        currentChoiceEvent = null;
        selectedChoiceIndex = 0;
        lastChoiceWasCorrect = false;

        ClearChoices();

        ShowDialoguePanel(false);
        ShowChoicesPanel(true);
        ShowContinueIcon(false);

        questionText.text = currentQuestion.questionText;

        if (currentQuestion.options == null || currentQuestion.options.Length == 0)
        {
            Debug.LogError( $"DialogueSystem: La pregunta {questionID} no tiene opciones." );
            EndChoice();

            return;
        }

        for (int i = 0; i < currentQuestion.options.Length; i++)
        {
            GameObject buttonObject = Instantiate(choiceButtonPrefab, choiceContainer);

            if (buttonObject == null) continue;

            StandartButton button = buttonObject.GetComponent<StandartButton>();

            if (button == null)
            {
                Debug.Log("destruir");
                Destroy(buttonObject);
                continue;
            }

            int choiceIndex = i;

            button.SetAnswer( currentQuestion.options[choiceIndex], choiceIndex);
            button.button.onClick.AddListener(() =>{ OnChoiceClicked(choiceIndex);});

            choiceButtons.Add(buttonObject);
        }

        UpdateSelectedChoiceUI();

        GameManager.instance.InitialGameEnd();
        GameManager.instance.MovingCamera(false);

        currentDialogueState = DialogueState.ChoicePresenting;
    }

    private void OnChoiceClicked(int choiceIndex)
    {
        if (currentDialogueState != DialogueState.ChoicePresenting)
            return;

        selectedChoiceIndex = choiceIndex;

        UpdateSelectedChoiceUI();

        ConfirmSelectedChoice(choiceIndex);
    }

    private QuestionData FindQuestion(int questionID)
    {
        if (questionsDatabase == null)  return null;

        return questionsDatabase.questions.Find(question =>
            question != null &&
            question.QuestionID == questionID
        );
    }

    private void ConfirmSelectedChoice(int choiceIndex)
    {
        if (currentDialogueState != DialogueState.ChoicePresenting) return;
        if (currentQuestion == null) return;
        if (choiceIndex < 0 || choiceIndex >= currentQuestion.options.Length)
            return;

        QuestionEventEntry questionEvent = questionEvents.Find(entry =>
                entry != null &&
                entry.id == currentQuestion.QuestionID
            );

        currentChoiceEvent = null;

        if (questionEvent != null)
        {
            QuestionChoiceEvent choiceEvent = questionEvent.choiceEvents.Find(entry =>
                    entry != null &&
                    entry.optionIndex == choiceIndex
                );

            if (choiceEvent != null)
                currentChoiceEvent = choiceEvent.onSelected;
        }

        if (!currentQuestion.useFeedback)
        {
            EndChoice();
            return;
        }

        lastChoiceWasCorrect = choiceIndex == currentQuestion.correctAnswerIndex;

        string feedback = lastChoiceWasCorrect
                ? currentQuestion.correctFeedback
                : currentQuestion.incorrectFeedback;

        StartCoroutine(TypeChoiceFeedback(feedback));
    }

    private IEnumerator TypeChoiceFeedback(string feedback)
    {
        ShowChoicesPanel(false);
        ShowDialoguePanel(true);
        ShowContinueIcon(false);

        dialogueText.text = string.Empty;

        currentDialogueState =  DialogueState.ChoiceFeedbackTyping;

        foreach (char character in feedback)
        {
            if (currentDialogueState != DialogueState.ChoiceFeedbackTyping)
            {
                dialogueText.text = feedback;
                break;
            }

            dialogueText.text += character;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (currentDialogueState == DialogueState.ChoiceFeedbackTyping)
        {
            currentDialogueState = DialogueState.ChoiceFeedbackFinished;
            ShowContinueIcon(true);
        }
    }

    private void CompleteChoiceFeedback()
    {
        if (currentQuestion == null) return;

        StopAllCoroutines();

        string feedback = lastChoiceWasCorrect
                ? currentQuestion.correctFeedback
                : currentQuestion.incorrectFeedback;

        dialogueText.text = feedback;

        ShowContinueIcon(true);

        currentDialogueState = DialogueState.ChoiceFeedbackFinished;
    }

    public void ScrollChoice(float scroll)
    {
        if (currentDialogueState != DialogueState.ChoicePresenting) return;
        if (choiceButtons.Count == 0) return;

        int previousIndex = selectedChoiceIndex;

        if (scroll > 0)
        {
            selectedChoiceIndex--;
            if (selectedChoiceIndex < 0)  selectedChoiceIndex = choiceButtons.Count - 1;
        }
        else if (scroll < 0)
        {
            selectedChoiceIndex++;

            if (selectedChoiceIndex >= choiceButtons.Count)
                selectedChoiceIndex = 0;
        }

        if (selectedChoiceIndex != previousIndex)
            UpdateSelectedChoiceUI();
    }

    private void OnChoiceButtonHovered(int index)
    {
        if (currentDialogueState != DialogueState.ChoicePresenting) return;
        if (index < 0 || index >= choiceButtons.Count) return;

        selectedChoiceIndex = index;
        UpdateSelectedChoiceUI();
    }

    private void UpdateSelectedChoiceUI()
    {
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            if (choiceButtons[i] == null) continue;

            StandartButton button = choiceButtons[i].GetComponent<StandartButton>();

            if (button != null) button.SetSelected( i == selectedChoiceIndex);
        }
    }

    private void ClearChoices()
    {
        foreach (GameObject button in choiceButtons)
            if (button != null)
                Destroy(button);

        choiceButtons.Clear();
        selectedChoiceIndex = 0;
    }

    public void EndDialogue()
    {
        if (currentDialogue == null)
        {
            ResetDialogueState();
            return;
        }

        currentDialogueState = DialogueState.DialogueEnding;
        StopAllCoroutines();

        int finishedDialogueID = currentDialogue.DialogueID;

        ClearChoices();
        currentLine = 0;
        dialogueText.text = string.Empty;
        questionText.text = string.Empty;

        ShowDialoguePanel(false);
        ShowChoicesPanel(false);
        ShowContinueIcon(false);

        GameManager.instance.InitialGameStart();
        GameManager.instance.MovingCamera(true);

        currentDialogue = null;

        currentDialogueState = DialogueState.None;
        InvokeDialogueFinished( finishedDialogueID );
    }

    private void InvokeDialogueFinished(int dialogueID)
    {
        DialogueEventEntry entry = dialogueEvents.Find(eventEntry =>
                eventEntry != null &&
                eventEntry.id == dialogueID
            );

        entry?.onDialogueFinished?.Invoke();
    }

    public void EndChoice()
    {
        currentDialogueState = DialogueState.ChoiceEnding;
        StopAllCoroutines();

        UnityEvent choiceEvent = currentChoiceEvent;
        ClearChoices();

        currentLine = 0;

        dialogueText.text = string.Empty;
        questionText.text = string.Empty;

        ShowDialoguePanel(false);
        ShowChoicesPanel(false);
        ShowContinueIcon(false);

        currentQuestion = null;
        currentChoiceEvent = null;

        GameManager.instance.InitialGameStart();
        GameManager.instance.MovingCamera(true);

        currentDialogueState = DialogueState.None;

        choiceEvent?.Invoke();
    }

    private void ResetDialogueState()
    {
        StopAllCoroutines();
        ClearChoices();

        currentDialogue = null;
        currentQuestion = null;
        currentChoiceEvent = null;

        currentLine = 0;

        dialogueText.text = string.Empty;
        questionText.text = string.Empty;

        ShowDialoguePanel(false);
        ShowChoicesPanel(false);
        ShowContinueIcon(false);

        currentDialogueState = DialogueState.None;
    }

    public void ShowDialoguePanel(bool state)
    {
        dialoguePanel.SetActive(state);
    }

    public void ShowChoicesPanel(bool state)
    {
        choicesPanel.SetActive(state);
    }

    public void ShowContinueIcon(bool state)
    {
        continueIcon.SetActive(state);
    }
}