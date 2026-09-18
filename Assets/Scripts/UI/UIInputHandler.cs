using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputHandler : MonoBehaviour
{
    private PlayerControls controls;
    public static event Action OnBoatSkipPressed;
    public static event Action OnInteractPressed;
    public static event Action OnRemoveItemPressed;
    public static event Action<int> OnSlotKeyPressed;

    public static event Action<float> OnScrollDialogueChoices;
    public static event Action OnSkipDialoguePressed;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void Update()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                OnSlotKeyPressed?.Invoke(i);
            }
        }
    }

    private void OnEnable()
    {
        controls.UI.Enable();
        controls.UI.Pause.performed += OnPauseInput;
        controls.UI.AdvanceDialogue.performed += OnAdvanceDialogueInput;
        controls.UI.Interact.performed += OnInteractInput;
        controls.UI.ScrollInventory.performed += OnScrollInventory;
        controls.UI.ToggleDescription.performed += OnToggleDescription;
        controls.UI.RemoveItem.performed += OnRemoveItem;
        controls.UI.SkipBoat.performed += ctx => OnBoatSkipPressed?.Invoke();
        controls.UI.SkipDialogue.performed += OnSkipDialogueInput;
        controls.UI.OnScrollDialogueChoices.performed += OnScrollDialogueChoicesInput;
    }

    private void OnDisable()
    {
        controls.UI.Pause.performed -= OnPauseInput;
        controls.UI.AdvanceDialogue.performed -= OnAdvanceDialogueInput;
        controls.UI.Interact.performed -= OnInteractInput;
        controls.UI.ScrollInventory.performed -= OnScrollInventory;
        controls.UI.ToggleDescription.performed -= OnToggleDescription;
        controls.UI.RemoveItem.performed -= OnRemoveItem;
        controls.UI.SkipBoat.performed -= ctx => OnBoatSkipPressed?.Invoke();
        controls.UI.OnScrollDialogueChoices.performed -= OnScrollDialogueChoicesInput;
        controls.UI.SkipDialogue.performed -= OnSkipDialogueInput;
        controls.UI.Disable();
    }

    private void OnPauseInput(InputAction.CallbackContext ctx)
    {
        if (UIManager.instance.optionsPanel.activeSelf || UIManager.instance.dialoguePanel.activeSelf || UIManager.instance.choicesPanel.activeSelf)
        {
            OnSkipDialoguePressed?.Invoke();
            return;
        }
        GameManager.instance.GamePause();
    }

    private void OnScrollDialogueChoicesInput(InputAction.CallbackContext ctx)
    {
        if (DialogueSystem.instance != null && DialogueSystem.instance.GetCurrentDialogueState() == DialogueState.ChoicePresenting)
        {
            float scroll = ctx.ReadValue<Vector2>().y;
            OnScrollDialogueChoices?.Invoke(scroll);
        }
    }

    private void OnAdvanceDialogueInput(InputAction.CallbackContext ctx)
    {
        if (UIManager.instance.pausePanel.activeSelf || UIManager.instance.optionsPanel.activeSelf) return;
        DialogueSystem.instance?.TryAdvance();
    }

    private void OnInteractInput(InputAction.CallbackContext ctx)
    {
        OnInteractPressed?.Invoke();
    }

    private void OnScrollInventory(InputAction.CallbackContext ctx)
    {
        if (UIManager.instance.InvetoryPanel.activeSelf)
        {
            float scroll = ctx.ReadValue<Vector2>().y;
            InventoryManager.Instance.ScrollSlot(scroll);
        }
    }

    private void OnToggleDescription(InputAction.CallbackContext ctx)
    {
        InventoryManager.Instance.ToggleDescription();
    }

    private void OnRemoveItem(InputAction.CallbackContext ctx)
    {
        OnRemoveItemPressed?.Invoke();
    }

    private void OnSkipDialogueInput(InputAction.CallbackContext ctx)
    {
        if (UIManager.instance.pausePanel.activeSelf || UIManager.instance.optionsPanel.activeSelf || UIManager.instance.creditsPanel) return;

        if (DialogueSystem.instance.GetCurrentDialogueState() == DialogueState.DialogueTyping ||
             DialogueSystem.instance.GetCurrentDialogueState() == DialogueState.ChoiceFeedbackTyping ||
             DialogueSystem.instance.GetCurrentDialogueState() == DialogueState.DialogueLineFinished)
        {
            OnSkipDialoguePressed?.Invoke();
        }
    }
}
