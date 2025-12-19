using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
//using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue")]
public class DialogueData : ScriptableObject
{
    [TextArea(4, 6)] public List<string> dialogueLines;

    [SerializeField] private int dialogueID;
    public int DialogueID => dialogueID;
}
