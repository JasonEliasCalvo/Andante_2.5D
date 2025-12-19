using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StandartButton : MonoBehaviour, IPointerEnterHandler
{
    public TextMeshProUGUI answer;
    public Button button;
    private int buttonIndex;
    public GameObject selectionIndicator;

    public static event Action<int> OnChoiceButtonHoverEnter;
    public void SetAnswer(string answerText, int index)
    {
        if (answer != null)
        {
            answer.text = answerText;
        }
        buttonIndex = index; 
        ;
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(isSelected);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DialogueSystem.instance != null && DialogueSystem.instance.GetCurrentDialogueState() == DialogueState.ChoicePresenting)
        {
            OnChoiceButtonHoverEnter?.Invoke(buttonIndex);
        }
    }
}
