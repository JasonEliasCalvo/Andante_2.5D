using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypingController : MonoBehaviour
{
    [SerializeField] private List<string> _targetWord;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI wordText;

    private int currentLetter = 0;
    private bool isTypingGameActive = false;

    public TextMeshProUGUI MessageText { get => messageText; set => messageText = value; }
    public int CurrentLetter { get => currentLetter; set => currentLetter = value; }
    public bool IsTypingGameActive { get => isTypingGameActive; set => isTypingGameActive = value; }
    public TextMeshProUGUI WordText { get => wordText; set => wordText = value; }
    public List<string> TargetWord { get => _targetWord; set => _targetWord = value; }

    private void Update()
    {
        if (IsTypingGameActive)
        {
            OnTextSubmitted();
        }
    }

    protected virtual void OnTextSubmitted()
    {

    }

    public char GetPressedKey()
    {
        if (Input.GetKeyDown(KeyCode.A)) return 'A';
        if (Input.GetKeyDown(KeyCode.B)) return 'B';
        if (Input.GetKeyDown(KeyCode.C)) return 'C';
        if (Input.GetKeyDown(KeyCode.D)) return 'D';
        if (Input.GetKeyDown(KeyCode.E)) return 'E';
        if (Input.GetKeyDown(KeyCode.F)) return 'F';
        if (Input.GetKeyDown(KeyCode.G)) return 'G';
        if (Input.GetKeyDown(KeyCode.H)) return 'H';
        if (Input.GetKeyDown(KeyCode.I)) return 'I';
        if (Input.GetKeyDown(KeyCode.J)) return 'J';
        if (Input.GetKeyDown(KeyCode.K)) return 'K';
        if (Input.GetKeyDown(KeyCode.L)) return 'L';
        if (Input.GetKeyDown(KeyCode.M)) return 'M';
        if (Input.GetKeyDown(KeyCode.N)) return 'N';
        if (Input.GetKeyDown(KeyCode.O)) return 'O';
        if (Input.GetKeyDown(KeyCode.P)) return 'P';
        if (Input.GetKeyDown(KeyCode.Q)) return 'Q';
        if (Input.GetKeyDown(KeyCode.R)) return 'R';
        if (Input.GetKeyDown(KeyCode.S)) return 'S';
        if (Input.GetKeyDown(KeyCode.T)) return 'T';
        if (Input.GetKeyDown(KeyCode.U)) return 'U';
        if (Input.GetKeyDown(KeyCode.V)) return 'V';
        if (Input.GetKeyDown(KeyCode.W)) return 'W';
        if (Input.GetKeyDown(KeyCode.X)) return 'X';
        if (Input.GetKeyDown(KeyCode.Y)) return 'Y';
        if (Input.GetKeyDown(KeyCode.Z)) return 'Z';
        if (Input.GetKeyDown(KeyCode.Space)) return ' ';

        else return '\0';
    }
}
