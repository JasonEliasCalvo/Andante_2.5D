using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "MemoryGame/Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite cardFront;
}