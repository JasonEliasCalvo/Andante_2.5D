using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HappyAndAngryGame : MonoBehaviour
{
    public GameObject panel;
    public List<CharacterData> characters;
    public Image characterImage;
    public DropSlot slotGender;
    public DropSlot slotEmocion;

    private CharacterData currentCharacter;
    private int characterIndex;

    public TextMeshProUGUI mensajeResultado;
    public bool isCheckingMatch;

    public float RestTime;
    public UnityEvent onHappyGame;

    [SerializeField] AudioClip correctSound;
    [SerializeField] AudioClip incorrectSound;
    [SerializeField] AudioClip victorySound;

    private void Update()
    {
        CheckSlots();
    }

    public void StartHappyGame()
    {
        characterIndex = 0;
        isCheckingMatch = false;

        GameManager.instance.InitialGameEnd();
        GameManager.instance.MovingCamera(false);
        NextCharacter();
    }

    private void ResetDraggableIcons()
    {
        DraggableIcon[] icons = FindObjectsByType<DraggableIcon>(FindObjectsSortMode.None);

        foreach (DraggableIcon icon in icons)
        {
            icon.ResetPosition();
        }

        slotGender.gender = GenderType.none;
        slotEmocion.emotion = EmotionType.none;
    }

    private void CheckSlots()
    {
        if (slotEmocion.emotion != EmotionType.none && slotGender.gender != GenderType.none && !isCheckingMatch)
        {
            isCheckingMatch = true;
            Debug.Log("Chekeo");
            CheckAnswer();
        }
        else
        {
            Debug.Log("No check");
        }
    }


    public void CheckAnswer()
    {
        if (slotGender.gender == currentCharacter.gender && slotEmocion.emotion == currentCharacter.emotion)
        {
            AudioManager.Instance.PlaySound(correctSound);
            StartCoroutine(ShowResult(true));
        }
        else
        {
            AudioManager.Instance.PlaySound(incorrectSound);
            StartCoroutine(ShowResult(false));
        }
    }

    private IEnumerator ShowResult(bool isCorrect)
    {
        mensajeResultado.text = isCorrect ? "¡Correcto!" : "¡Incorrecto!";

        if (!isCorrect)
            mensajeResultado.transform.DOShakeScale(0.5f, 0.2f, 10, 90);

        yield return new WaitForSeconds(RestTime);

        if (isCorrect)
        {
            if (characterIndex < characters.Count - 1)
            {
                characterIndex++;
                NextCharacter();
            }
            else
            {
                StartCoroutine(ShowWin()); 
            }
        }
        else
        {
            mensajeResultado.text = "La Respuesta es.....";
            ResetDraggableIcons();
            isCheckingMatch = false;
        }
    }

    public void NextCharacter()
    {
        GameManager.instance.HidePanels();
        StartCoroutine(AnimarCambioDePersonaje());
    }

    private IEnumerator AnimarCambioDePersonaje()
    {
        characterImage.transform.DOScale(Vector3.zero, 0.3f);
        yield return new WaitForSeconds(0.3f);

        UIManager.instance.AnimatePanelIn(panel);
        currentCharacter = characters[characterIndex];
        characterImage.sprite = currentCharacter.characterImage;

        characterImage.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        mensajeResultado.text = "La Respuesta es.....";
        ResetDraggableIcons();
        isCheckingMatch = false;
    }

    private IEnumerator ShowWin()
    {
        mensajeResultado.text = "¡Ganaste!";
        AudioManager.Instance.PlaySound(victorySound);

        yield return new WaitForSeconds(RestTime);

        UIManager.instance.AnimatePanelOut(panel);
        GameManager.instance.InitialGameStart();
        GameManager.instance.MovingCamera(true);
        ResetDraggableIcons();
        onHappyGame.Invoke();
    }
}
