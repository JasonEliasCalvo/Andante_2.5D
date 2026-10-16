using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MemoryGameController : MonoBehaviour
{
    [SerializeField] private List<CardData> cardDataList;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform boardParent;
    [SerializeField] private int columns = 4;
    [SerializeField] private float spacing = 1f;

    private List<MemoryCard> cards = new List<MemoryCard>();
    private Vector3 StartRotation = new Vector3(0, 0, -90);
    private MemoryCard firstCard, secondCard;
    public static bool isCheckingMatch = false;
    [SerializeField] private GameObject cam1;
    [SerializeField] private GameObject cam2;

    private Timer timer;
    [SerializeField] AudioClip incorrectSound;
    public UnityEvent onGameFinished;
    public static Camera memoryInputCamera;

    public void StartMemoryGame()
    {

        firstCard = null;
        secondCard = null;
        isCheckingMatch = false;

        if (timer == null)
        {
            timer = GameManager.instance.GetTimer();
        }

        if (timer != null)
        {
            // Evita duplicar la suscripción si el minijuego se reinicia.
            timer.eventEndTime -= ResetMemoryGame;
            timer.eventEndTime += ResetMemoryGame;

            timer.Initiate(90f);
        }

        GameManager.instance.InitialGameEnd();
        GameManager.instance.MovingCamera(false);

        UIManager.instance.ShowCursor(true);
        UIManager.instance.ShowInteractablePanel(false);

        GameManager.instance.StartFadeOut(() =>
        {
            foreach (Transform child in boardParent)
            {
                if (child.TryGetComponent<MemoryCard>(out _))
                {
                    Destroy(child.gameObject);
                }
            }

            cam1.SetActive(true);
            cam2.SetActive(false);

            GameManager.instance.StartFadeIn(() =>
            {
                GenerateCards();
            });
        });
    }

    private void Start()
    {
        timer = GameManager.instance.GetTimer();
        if (cam1 != null) memoryInputCamera = cam1.GetComponent<Camera>();
    }

    private void OnEnable()
    {
        MemoryCard.OnCardFlipped += CheckMatch;
    }

    private void OnDisable()
    {
        MemoryCard.OnCardFlipped -= CheckMatch;
    }

    public void ResetMemoryGame()
    {
        StopAllCoroutines();

        foreach (Transform child in boardParent)
        {
            if (child.TryGetComponent<MemoryCard>(out _))
            {
                Destroy(child.gameObject);
            }
        }

        cards.Clear();

        firstCard = null;
        secondCard = null;
        isCheckingMatch = false;

        UIManager.instance.ShowInteractablePanel(false);
        StartMemoryGame();
    }

    public void GenerateCards()
    {
        List<CardData> allCards = new List<CardData>(cardDataList);
        allCards.AddRange(cardDataList);
        Shuffle(allCards);

        StartCoroutine(GenerateCardsSlowly(allCards));
    }

    private IEnumerator GenerateCardsSlowly(List<CardData> allCards)
    {
        cards.Clear();

        for (int i = 0; i < allCards.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            GameManager.instance.HidePanels();
            Vector3 position = boardParent.position + new Vector3(col * spacing, 0, row * spacing);
            GameObject newCard = Instantiate(cardPrefab, position, Quaternion.Euler(StartRotation), boardParent);
            MemoryCard memoryCard = newCard.GetComponent<MemoryCard>();
            memoryCard.SetupCard(allCards[i]);
            memoryCard.enabled = true;
            cards.Add(memoryCard);

            yield return new WaitForSeconds(0.2f);

            StartCoroutine(AutoFlipCard(memoryCard, 3f));
        }
        GameManager.instance.GetTimer().Reboot();
        UIManager.instance.ShowTimerPanel(true);
    }

    private IEnumerator AutoFlipCard(MemoryCard card, float delay)
    {
        card.FlipCard(false);
        yield return new WaitForSeconds(delay);
        card.HideCard();
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CardData temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void CheckMatch(MemoryCard card)
    {
        if (isCheckingMatch) return;

        if (firstCard == null)
        {
            firstCard = card;
        }
        else
        {
            secondCard = card;
            isCheckingMatch = true;
            StartCoroutine(CompareCards());
        }
    }

    private IEnumerator CompareCards()
    {
        yield return new WaitForSeconds(1f);

        if (firstCard.cardData.cardName == secondCard.cardData.cardName)
        {
            firstCard.MarkAsMatched();
            secondCard.MarkAsMatched();

            firstCard.PlayMatchFeedback();
            secondCard.PlayMatchFeedback();

            CheckGameOver();
        }
        else
        {
            firstCard.HideCard();
            secondCard.HideCard();
            AudioManager.Instance.PlaySound(incorrectSound);
        }

        firstCard = null;
        secondCard = null;
        isCheckingMatch = false;
    }

    private void CheckGameOver()
    {
        if (cards.All(card => card.IsMatched()))
        {
            EndMemoryGame();
            UIManager.instance.ShowTimerPanel(false);
        }
    }

    public void EndMemoryGame()
    {
        if (timer != null)
        {
            timer.eventEndTime -= ResetMemoryGame;
            timer.Stop();
        }

        StartCoroutine(EndMemoryGameSequence());
    }

    private IEnumerator EndMemoryGameSequence()
    {
        foreach (MemoryCard card in cards)
        {
            card.enabled = false;
        }

        yield return new WaitForSeconds(1f);

        GameManager.instance.StartFadeOut(() =>
        {
            cam1.SetActive(false);
            cam2.SetActive(true);

            GameManager.instance.StartFadeIn(() =>
            {
                cards.Clear();

                GameManager.instance.InitialGameStart();
                GameManager.instance.MovingCamera(true);

                UIManager.instance.ShowCursor(false);

                onGameFinished?.Invoke();

                Debug.Log("Memory game completed.");
            });
        });
    }
}
