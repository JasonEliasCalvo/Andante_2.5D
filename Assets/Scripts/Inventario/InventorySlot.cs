using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public GameObject selector;
    public Item currentItem;
    public TextMeshProUGUI numberText;

    public void Awake()
    {
        icon.enabled = false;
    }

    public void SetSlotNumber(int index)
    {
        if (numberText != null)
            numberText.text = (index + 1).ToString();
    }

    public void SetItem(Item item)
    {
        currentItem = item;
        if (item != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
        }
        else
        {
            ClearItem();
        }
    }


    public void ClearItem()
    {
        currentItem = null;
        icon.sprite = null;
        icon.enabled = false;
    }


    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public void SetSelected(bool selected)
    {
        selector.SetActive(selected);
        if (selected)
        {
            icon.transform.DOKill();
            icon.transform.localScale = Vector3.one * 1.2f;
            icon.transform.DOScale(Vector3.one, 0.4f)
                .SetEase(Ease.OutBounce);
        }
    }
}
