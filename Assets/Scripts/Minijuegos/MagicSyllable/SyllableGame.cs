using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SyllableGame : MonoBehaviour
{
    public UnityEvent onSyllablegame;
    [SerializeField] private List<WordData> wordList;
    [SerializeField] private AllSyllableData syllableDatas;
    [SerializeField] private int maxChoices = 4;
    [SerializeField] private int maxCorrectAnswers = 10;
    [SerializeField] private GameObject syllableButtonPrefab;
    [SerializeField] private TextMeshProUGUI syllableText;
    [SerializeField] private Transform syllableContainer;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Image syllableImage;
    [SerializeField] AudioClip correctSound;
    [SerializeField] AudioClip incorrectSound;
    [SerializeField] AudioClip victorySound;

    public bool randomOrder = true;
    private int currentWordIndex = 0;
    private int correctAnswers = 0;
    private int lives = 3;

    private List<WordData> remainingWords;
    public float RestTime;

    private void Start()
    {
        //GameManager.instance.eventSyllableGameStart += StartGame;
    }

    private void StartGame()
    {
        currentWordIndex = 0;
        correctAnswers = 0;
        lives = 3;
        livesText.text = "Vidas: " + lives;
        //UIManager.instance.ShowSyllableGamePanel(true);
        GameManager.instance.InitialGameEnd();
        GameManager.instance.MovingCamera(false);

        remainingWords = new List<WordData>(wordList);

        if (randomOrder)
        {
            remainingWords = remainingWords.OrderBy(x => Random.value).ToList();
        }

        ShowNextWord();
    }

    private void ShowNextWord()
    {
        GameManager.instance.HidePanels();
        if (correctAnswers >= maxCorrectAnswers || currentWordIndex >= remainingWords.Count)
        {
            StartCoroutine(SyllableGameWin());
            return;
        }

        if (lives <= 0)
        {
            SyllableGameEnd();
            return;
        }

        WordData currentWord = remainingWords[currentWordIndex];
        string newText = currentWord.fullWord.Replace(currentWord.correctSyllable, "...");
        Sprite newSprite = currentWord.image;

        AnimateSyllableUI(newText, newSprite, () =>
        {
            AnimateOldButtons(() => GenerateChoices(currentWord));
        });
    }

    private void AnimateOldButtons(System.Action onComplete)
    {
        var children = new List<Transform>();
        foreach (Transform child in syllableContainer) children.Add(child);

        if (children.Count == 0)
        {
            onComplete.Invoke();
            return;
        }

        int completed = 0;
        foreach (var child in children)
        {
            child.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
            {
                Destroy(child.gameObject);
                completed++;
                if (completed == children.Count)
                {
                    onComplete.Invoke();
                }
            });
        }
    }

    private void AnimateSyllableUI(string newText, Sprite newSprite, System.Action onComplete)
    {
        var seq = DOTween.Sequence();

        seq.Append(syllableText.transform.DOScale(Vector3.zero, 0.15f));
        seq.Join(syllableImage.transform.DOScale(Vector3.zero, 0.15f));

        seq.AppendCallback(() =>
        {
            syllableText.text = newText;
            syllableImage.sprite = newSprite;
        });

        seq.Append(syllableText.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        seq.Join(syllableImage.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

        seq.OnComplete(() => onComplete?.Invoke());
    }

    private void GenerateChoices(WordData word)
    {
        List<string> choices = new List<string>(word.syllables);

        while (choices.Count < maxChoices)
        {
            string fakeSyllable = syllableDatas.allSyllables[Random.Range(0, syllableDatas.allSyllables.Count)];

            if (!choices.Contains(fakeSyllable) && !word.fullWord.Contains(fakeSyllable))
            {
                choices.Add(fakeSyllable);
            }
        }

        choices = choices.OrderBy(x => Random.value).ToList();

        foreach (var choice in choices)
        {
            GameObject button = Instantiate(syllableButtonPrefab, syllableContainer);
            button.transform.localScale = Vector3.zero;
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice;
            button.GetComponent<Button>().onClick.AddListener(() => CheckAnswer(choice, word.correctSyllable));
            button.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
    }

    private void CheckAnswer(string chosen, string correct)
    {
        if (chosen == correct)
        {
            correctAnswers++;
            AudioManager.Instance.PlaySound(correctSound);
        }
        else
        {
            lives--;
            AudioManager.Instance.PlaySound(incorrectSound);
            DOTween.Sequence()
                .Append(livesText.transform.DOShakeScale(0.3f, 0.3f))
                .AppendCallback(UpdateLivesUI);
        }

        currentWordIndex++;
        ShowNextWord();
    }

    private void UpdateLivesUI()
    {
        livesText.text = "Vidas: " + lives;
    }

    private IEnumerator SyllableGameWin()
    {
        AudioManager.Instance.PlaySound(victorySound);

        DOTween.Sequence().Append(syllableText.transform.DOScale(Vector3.zero, 0.15f));

        DOTween.Sequence().AppendCallback(() =>
        {
            syllableText.text = "Ganaste";
        });

        DOTween.Sequence().Append(syllableText.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
    
        DOTween.Sequence().OnComplete(() => onSyllablegame?.Invoke());

        yield return new WaitForSeconds(RestTime);

        SyllableGameEnd();
    }

    private void SyllableGameEnd()
    {
        //UIManager.instance.ShowSyllableGamePanel(false);
        GameManager.instance.InitialGameStart();
        GameManager.instance.MovingCamera(true);
    }
}
