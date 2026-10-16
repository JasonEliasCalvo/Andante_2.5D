using UnityEngine;
using UnityEngine.Events;
//using static UnityEditor.Progress;

[CreateAssetMenu(fileName = "NewQuestion", menuName = "Dialogue System/Question")]
public class QuestionData : ScriptableObject
{
    [TextArea(3, 3)] public string questionText;
    [TextArea(2, 2)] public string[] options;

    [Header("Feedback")]
    public bool useFeedback = false;
    [TextArea(3, 3)] public string correctFeedback = "¡Correcto! Puedes continuar.";
    [TextArea(3, 3)] public string incorrectFeedback = "Incorrecto. Intenta de nuevo.";

    [Header("Quiz")]
    public int correctAnswerIndex;

    [SerializeField] private int questionID;
    public int QuestionID => questionID;
}