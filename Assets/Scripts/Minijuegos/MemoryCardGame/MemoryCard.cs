using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MemoryCard : MonoBehaviour
{
    public CardData cardData;
    public GameObject frontSide;

    private bool isFlipped = false;
    [SerializeField] private bool isMatched = false;
    [SerializeField] private Vector3 HideRotation = new Vector3(0, 0, -90);
    [SerializeField] private Vector3 FlipRotation = new Vector3(180, 0, -90);

    [SerializeField] private float duration = 1f;
    public static UnityAction<MemoryCard> OnCardFlipped;

    [SerializeField] AudioClip victorySound;

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (MemoryGameController.memoryInputCamera == null)
            return;

        if (MemoryGameController.isCheckingMatch)
            return;

        Ray ray = MemoryGameController.memoryInputCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            MemoryCard card = hit.collider.GetComponentInParent<MemoryCard>();
            if (card != null) card.FlipCard(true);
        }
    }

    public void SetupCard(CardData data)
    {
        cardData = data;
        frontSide.GetComponent<SpriteRenderer>().sprite = data.cardFront;
        HideCard();
    }

    public void FlipCard(bool triggerCheck = true)
    {
        if (isFlipped || isMatched || MemoryGameController.isCheckingMatch) return;

        isFlipped = true;
        transform.DORotate(FlipRotation, duration, RotateMode.Fast);

        if (triggerCheck)
            OnCardFlipped?.Invoke(this);
    }


    public void HideCard()
    {
        if (isMatched) return;

        transform.DORotate(HideRotation, duration, RotateMode.Fast);
        isFlipped = false;
    }

    public void PlayMatchFeedback()
    {
        AudioManager.Instance.PlaySound(victorySound);
        transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 6, 1f);
    }

    public void MarkAsMatched()
    {
        isMatched = true;
    }

    public bool IsMatched() { return isMatched; }
}
