using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class EnumType
{
    public bool isGender;
    public bool isEmotion;
}

public class DropSlot : MonoBehaviour, IDropHandler
{
    public GenderType gender;
    public EmotionType emotion;

    public EnumType enumType;
    private DraggableIcon oldIcon;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableIcon newIcon = eventData.pointerDrag.GetComponent<DraggableIcon>();

        if (newIcon == null)
            return;

        if (transform.childCount > 0)
        {
            DraggableIcon oldIcon = transform.GetChild(0).GetComponent<DraggableIcon>();
            if (oldIcon != null && oldIcon != newIcon)
            {
                oldIcon.ResetPosition();
            }
        }

        if (enumType.isGender == newIcon.enumType.isGender)
        {
            newIcon.RectTransform.SetParent(transform);
            newIcon.RectTransform.position = transform.position;
            gender = newIcon.gender;
            AdjustIconToSlot(newIcon.RectTransform);
            newIcon.MarkAsDroppedInSlot();
        }

        if (enumType.isEmotion == newIcon.enumType.isEmotion)
        {
            newIcon.RectTransform.SetParent(transform);
            newIcon.RectTransform.position = transform.position;
            emotion = newIcon.emotion;
            AdjustIconToSlot(newIcon.RectTransform);
            newIcon.MarkAsDroppedInSlot();
        }
    }

    private void AdjustIconToSlot(RectTransform iconTransform)
    {
        var icon = iconTransform.GetComponent<DraggableIcon>();
        if (icon != null)
            icon.canvasGroup.blocksRaycasts = false;

        iconTransform.DOScale(0.8f, 0.1f).SetEase(Ease.InSine).OnComplete(() =>
        {
            iconTransform.anchorMin = new Vector2(0, 0);
            iconTransform.anchorMax = new Vector2(1, 1);
            iconTransform.offsetMin = Vector2.zero;
            iconTransform.offsetMax = Vector2.zero;

            iconTransform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (icon != null)
                    icon.canvasGroup.blocksRaycasts = true;
            });
        });
    }
}