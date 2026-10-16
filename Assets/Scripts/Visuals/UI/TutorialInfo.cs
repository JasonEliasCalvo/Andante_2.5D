using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "NewTutorial", menuName = "Tutorial System/Tutorial Info")]
public class TutorialInfo : ScriptableObject
{
    public int tutorialID;
    public string title;
    [TextArea]
    public string instruction;
    public Sprite tutorialImage;
    public VideoClip tutorialVideo;
}