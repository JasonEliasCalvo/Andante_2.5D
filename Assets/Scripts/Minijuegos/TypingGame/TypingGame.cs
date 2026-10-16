using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypingGame : TypingController
{
    [SerializeField] private Transform player;
    [SerializeField] private float speed;
    [SerializeField] private Transform startPlataformPosition;
    [SerializeField] private GameObject plataform;
    [SerializeField] private List<Transform> platforms;
    [SerializeField] private float distanceBetweenPlatforms;
    [SerializeField] private Vector3 generationDirection = Vector3.right;
    private Vector3 startPlayerPosition;
    private Vector3 newPlayerPosition;
    private Quaternion newPlayerRotation;
    private int _currentWord = 0;
    private int currentPlataform = 0;
    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;

    void Start()
    {
        //GameManager.instance.eventTypingGameStart += StartTypingGame;
        //GameManager.instance.eventTypingGameEnd += EndTypingGame;
        //GameManager.instance.eventTypingGameReset += ResetTypingGame;
    }

    private void StartTypingGame()
    {
        GameManager.instance.InitialGameEnd();
        startPlayerPosition = player.position;
        WordText.text = TargetWord[0];
        CreatePlatforms(_currentWord);
        MessageText.text = "Escibe la Palabra:";
        //UIManager.instance.ShowTypingPanel(true);
        //UIManager.instance.ShowTimerPanel(true);
        GameManager.instance.HidePanels();
        ResetTypingGame();
    }

    void CreatePlatforms(int currentWord)
    {
        _currentWord = currentWord;

        foreach (Transform platform in platforms)
        {
            Destroy(platform.gameObject);
        }
        platforms.Clear();

        Vector3 currentPosition = startPlataformPosition.position;
        for (int wordIndex = 0; wordIndex < TargetWord.Count; wordIndex++)
        {
            for (int i = 0; i < TargetWord[wordIndex].Length; i++)
            {
                GameObject newPlatform = Instantiate(plataform, currentPosition, Quaternion.identity);
                platforms.Add(newPlatform.transform);
                newPlatform.SetActive(false);
                currentPosition += generationDirection.normalized * distanceBetweenPlatforms;
            }
        }
        StartCoroutine(DelayedStart());
    }

    protected override void OnTextSubmitted()
    {
        if (CurrentLetter < TargetWord[_currentWord].Length)
        {
            char pressedKey = GetPressedKey();

            if (pressedKey != '\0')
            {
                if (pressedKey == TargetWord[_currentWord][CurrentLetter])
                {
                    platforms[currentPlataform].gameObject.SetActive(true);

                    newPlayerPosition = new Vector3(platforms[currentPlataform].position.x, platforms[currentPlataform].position.y, platforms[currentPlataform].position.z);
                    newPlayerRotation = Quaternion.LookRotation(generationDirection);

                    CurrentLetter++;
                    currentPlataform++;
                    if (CurrentLetter == TargetWord[_currentWord].Length)
                    {
                        _currentWord++;
                        if (_currentWord < TargetWord.Count)
                        {
                            GameManager.instance.HidePanels();
                            StartCoroutine(ShowTemporaryMessage("¡Has completado la palabra!", 5f));

                            CurrentLetter = 0;

                            WordText.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
                            {
                                WordText.text = TargetWord[_currentWord];
                                WordText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                            });

                            GameManager.instance.GetTimer().Reboot();

                            lastCheckpointPosition = newPlayerPosition;
                            lastCheckpointRotation = newPlayerRotation;
                        }
                        else
                        {
                            StartCoroutine(ShowMessage("¡Has completado el minijuego!"));
                            GameManager.instance.GetTimer().Stop();
                            //GameManager.instance.TypingGameEnd();

                        }
                    }
                }
                else
                {
                    StartCoroutine(ShowTemporaryMessage("¡Error! Comienza de nuevo.", 1.5f));
                    StartCoroutine(DelayedReset());
                }
            }
        }
    }

    private IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        Color originalColor = Color.black;

        MessageText.transform.DOScale(Vector3.zero, 0.2f);
        yield return new WaitForSeconds(0.2f);

        MessageText.text = message;
        if (message.Contains("Error"))
            MessageText.color = Color.red;

        MessageText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            if (message.Contains("Error"))
            {
                MessageText.transform.DOShakeScale(0.5f, 0.2f, 10, 90);
                MessageText.color = Color.red;
            }
        });

        yield return new WaitForSeconds(duration);

        MessageText.color = originalColor;

        MessageText.transform.DOScale(Vector3.zero, 0.2f);
        yield return new WaitForSeconds(0.2f);

        MessageText.text = "Escribe la Palabra:";
        MessageText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }


    private IEnumerator ShowMessage(string message)
    {
        MessageText.transform.DOScale(Vector3.zero, 0.2f);
        yield return new WaitForSeconds(0.2f);

        MessageText.text = message;
        MessageText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    private void FixedUpdate()
    {
        if (IsTypingGameActive)
        {
            player.position = Vector3.Lerp(player.position, newPlayerPosition, speed);
            player.rotation = Quaternion.Slerp(player.rotation, newPlayerRotation, speed);
        }
    }

    private void EndTypingGame()
    {
        IsTypingGameActive = false;
        StartCoroutine(IEEndTyping());
    }

    private IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(0.5f);
        ResetTypingGame();
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.1f);
        IsTypingGameActive = true;
    }

    void ResetTypingGame()
    {
        CurrentLetter = 0;

        Vector3 elevatedOffset = new Vector3(0, 2f, 0);

        if (_currentWord > 0)
        {
            player.position = lastCheckpointPosition + elevatedOffset;
            newPlayerPosition = lastCheckpointPosition;
            player.rotation = lastCheckpointRotation;
        }
        else
        {
            player.position = startPlayerPosition + elevatedOffset;
            newPlayerPosition = startPlayerPosition;
        }

        GameManager.instance.GetTimer().Reboot();

        int totalLetters = 0;
        for (int i = 0; i < _currentWord; i++)
        {
            totalLetters += TargetWord[i].Length;
        }

        for (int i = totalLetters; i < platforms.Count; i++)
        {
            platforms[i].gameObject.SetActive(false);
        }

        currentPlataform = totalLetters;
    }

    IEnumerator IEEndTyping()
    {
        yield return new WaitForSeconds(2f);
        GameManager.instance.InitialGameStart();
        //UIManager.instance.ShowTypingPanel(false);
        //UIManager.instance.ShowTimerPanel(false);
    }
}
