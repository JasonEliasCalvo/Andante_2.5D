using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New MoveSet", menuName = "Combat/ MoveSet")]
public class MoveSet : ScriptableObject
{
    [Header("Attacks")]
    public List<ActionData> actions;
}