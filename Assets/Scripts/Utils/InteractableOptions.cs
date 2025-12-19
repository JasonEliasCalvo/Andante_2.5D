using System.Collections;
using UnityEngine;

public enum InteractionType
{
    GenerateCards,
    StartDialogue,
    StartMoving,
    StopMoving,
    StartTypingGame,
    StartSyllableGame,
    StartHappyAndAngryGame,
}

public class InteractableOptions : MonoBehaviour
{
    private DialogueSystem dialogueSystem;

    [SerializeField] private InteractionType interactionType;
    public InteractionType InteractionType { get => interactionType; set => interactionType = value; }

    public int iD;

    public MovableObject movableObject;

    public bool possibleInteract = true;
    private bool isPlayerInTrigger = false;

    [SerializeField] private bool justAnInterraction = false;

    private float cooldownTimer = 0.2f;
    private float cooldownCounter = 0f;

    void Start()
    {
        dialogueSystem = FindFirstObjectByType<DialogueSystem>();
    }

    private void OnEnable()
    {
        UIInputHandler.OnInteractPressed += TryInteract;
    }

    private void OnDisable()
    {
        UIInputHandler.OnInteractPressed -= TryInteract;
    }

    private void Update()
    {
        if (dialogueSystem.GetCurrentDialogueState() != DialogueState.None)
        {
            cooldownCounter = 0f;
        }
        else
        {
            if (cooldownCounter < cooldownTimer)
            {
                cooldownCounter += Time.deltaTime;
            }
        }
    }

    private void TryInteract()
    {
        if (possibleInteract && isPlayerInTrigger && !UIManager.instance.IsPanelActive())
        {
            if (cooldownCounter < cooldownTimer || dialogueSystem.GetCurrentDialogueState() != DialogueState.None) return;

            UIManager.instance.ShowInteractablePanel(false);
            ExecuteInteraction();
            PlayerOutTrigger();
        }
    }

    private void ExecuteInteraction()
    {
        if (justAnInterraction) { StopInterract(); }

        switch (InteractionType)
        {
            case InteractionType.GenerateCards:
                GameManager.instance.MemoryGameStart();
                break;

            case InteractionType.StartDialogue:
                dialogueSystem?.StartDialogue(iD);
                break;

            case InteractionType.StartMoving:
                movableObject?.StartMoving();
                break;

            case InteractionType.StopMoving:
                movableObject?.StopMoving();
                break;

            case InteractionType.StartTypingGame:
                GameManager.instance.TypingGameStart();
                break;

            case InteractionType.StartSyllableGame:
                GameManager.instance.SyllableGameStart();
                break;

            case InteractionType.StartHappyAndAngryGame:
                GameManager.instance.HappyGameStart();
                break;
        }
    }

    public void PlayerInTrigger() => isPlayerInTrigger = true;
    public void PlayerOutTrigger() => isPlayerInTrigger = false;
    public void StartInterract() => possibleInteract = true;
    public void StopInterract() => possibleInteract = false;
    public void ShowInteractionPanel(bool state) => UIManager.instance.ShowInteractablePanel(state);
}