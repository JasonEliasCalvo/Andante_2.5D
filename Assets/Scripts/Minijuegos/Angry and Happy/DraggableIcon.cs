using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum GenderType
{
    none,
    male,
    Female
}

public enum EmotionType
{
    none,
    Angry,
    Happy,
}

public class DraggableIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    [SerializeField] private GameObject happyAndAngrypanel;
    private RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalOffsetMin;
    private Vector2 originalOffsetMax;
    private Vector3 originalScale;


    public GenderType gender;
    public EmotionType emotion;
    public EnumType enumType;
    public bool wasDroppedOnSlot;

    public RectTransform RectTransform { get => rectTransform; set => rectTransform = value; }
    public Vector3 OriginalPosition { get => originalPosition; set => originalPosition = value; }
    public Transform OriginalParent { get => originalParent; set => originalParent = value; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        originalPosition = rectTransform.position;
        OriginalParent = transform.parent;

        originalAnchorMin = rectTransform.anchorMin;
        originalAnchorMax = rectTransform.anchorMax;
        originalOffsetMin = rectTransform.offsetMin;
        originalOffsetMax = rectTransform.offsetMax;
        originalScale = rectTransform.localScale;

        ResetPosition();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (transform.parent != originalParent)
        {
            transform.SetParent(OriginalParent);
        }
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (!wasDroppedOnSlot)
        {
            ResetPosition();
        }

        wasDroppedOnSlot = false;
    }

    public void ResetPosition()
    {
        canvasGroup.blocksRaycasts = false;

        rectTransform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).OnComplete(() =>
        {
            rectTransform.anchorMin = originalAnchorMin;
            rectTransform.anchorMax = originalAnchorMax;
            rectTransform.offsetMin = originalOffsetMin;
            rectTransform.offsetMax = originalOffsetMax;
            rectTransform.position = originalPosition;
            transform.SetParent(originalParent);

            rectTransform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = true;
            });
        });
    }

    internal void MarkAsDroppedInSlot()
    {
        wasDroppedOnSlot = true;
    }
}

