using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Dialogue System/Question Database")]
public class QuestionDatabase : ScriptableObject
{
    public List<QuestionData> questions = new();
}

[Serializable]
public class QuestionChoiceEvent
{
    public int optionIndex;
    public UnityEvent onSelected;
}

[Serializable]
public class QuestionEventEntry
{
    public int id;

    public List<QuestionChoiceEvent> choiceEvents = new();
}