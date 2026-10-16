using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TutorialDatabase", menuName = "Tutorial System/Tutorial Database")]
public class TutorialDatabase : ScriptableObject
{
    public List<TutorialInfo> tutorials = new List<TutorialInfo>();

    public TutorialInfo GetTutorialByID(int tutorialID)
    {
        foreach (var tutorial in tutorials)
        {
            if (tutorial.tutorialID == tutorialID)
            {
                return tutorial;
            }
        }
        return null;
    }
}