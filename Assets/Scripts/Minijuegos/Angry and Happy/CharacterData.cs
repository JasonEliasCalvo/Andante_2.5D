using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "AngryAndHappy/Character")]
public class CharacterData : ScriptableObject
{
    public Sprite characterImage;
    public GenderType gender;
    public EmotionType emotion;
}
