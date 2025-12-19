
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventorySlot> slots = new List<InventorySlot>();
    public int selectedSlotIndex = 0;
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private Transform descriptionContent;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private float typingSpeed = 0.02f;
    private Coroutine descriptionCoroutine;
    private bool isTyping = false;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        UIInputHandler.OnSlotKeyPressed += HandleSlotKey;
    }

    private void OnDestroy()
    {
        UIInputHandler.OnSlotKeyPressed -= HandleSlotKey;
    }

    private void Start()
    {
        UpdateSelectedSlotUI();
        descriptionPanel.SetActive(false);

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetSlotNumber(i);
        }
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && descriptionPanel.activeSelf)
            ToggleDescription();
    }

    public void ScrollSlot(float scroll)
    {
        if (scroll != 0)
        {
            selectedSlotIndex = (selectedSlotIndex + (scroll > 0 ? 1 : -1) + slots.Count) % slots.Count;
            UpdateSelectedSlotUI();
        }
    }

    private void HandleSlotKey(int index)
    {
        if (index >= 0 && index < slots.Count)
        {
            selectedSlotIndex = index;
            UpdateSelectedSlotUI();
        }
    }

    private void UpdateSelectedSlotUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetSelected(i == selectedSlotIndex);
        }
    }

    public void RemoveItem()
    {
        if (slots[selectedSlotIndex].currentItem != null)
        {
            slots[selectedSlotIndex].ClearItem();
            CompactInventory();
        }
    }

    public void CompactInventory()
    {
        List<Item> items = new List<Item>();

        foreach (var slot in slots)
        {
            if (!slot.IsEmpty())
            {
                items.Add(slot.currentItem);
                slot.ClearItem();
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            slots[i].SetItem(items[i]);
        }

        UpdateSelectedSlotUI();
    }

    public bool AddItem(Item item)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(item);
                return true;
            }
        }
        return false;
    }

    public void ToggleDescription()
    {
        if(UIManager.instance.dialoguePanel.activeSelf) return;

        descriptionText.text = string.Empty;

        if (slots[selectedSlotIndex].currentItem != null)
        {
            if (!descriptionPanel.activeSelf)
            {
                descriptionPanel.SetActive(true);

                if (descriptionCoroutine != null)
                    StopCoroutine(descriptionCoroutine);

                descriptionContent.localScale = Vector3.zero;
                descriptionContent.localPosition = new Vector3(
                    descriptionContent.localPosition.x,
                    10f,
                    descriptionContent.localPosition.z
                );

                descriptionContent.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                descriptionContent.DOLocalMoveY(200f, 0.3f).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    descriptionCoroutine = StartCoroutine(ShowItemDescription(slots[selectedSlotIndex].currentItem.description));
                });
            }
            else
            {
                if (isTyping)
                {
                    if (descriptionCoroutine != null)
                        StopCoroutine(descriptionCoroutine);

                    descriptionText.text = slots[selectedSlotIndex].currentItem.description;
                    isTyping = false;
                }
                else
                {
                    CloseDescriptionPanel();
                }
            }
        }
        else
        {
            if (descriptionPanel.activeSelf)
            {
                CloseDescriptionPanel();
            }
        }
    }

    private void CloseDescriptionPanel()
    {
        descriptionContent.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
        descriptionContent.DOLocalMoveY(10f, 0.2f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            descriptionPanel.SetActive(false);
            if (descriptionCoroutine != null)
                StopCoroutine(descriptionCoroutine);
            isTyping = false;
        });
    }

    private IEnumerator ShowItemDescription(string text)
    {
        isTyping = true;
        descriptionText.text = string.Empty;

        foreach (char ch in text)
        {
            descriptionText.text += ch;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}