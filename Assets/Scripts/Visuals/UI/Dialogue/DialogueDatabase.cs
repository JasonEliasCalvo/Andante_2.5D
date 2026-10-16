using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Dialogue System/Dialogue Database")]
public class DialogueDatabase : ScriptableObject
{
    public List<DialogueData> dialogues = new();
}

[System.Serializable]
public class DialogueEventEntry
{
    public int id;
    public UnityEvent onDialogueFinished;
}