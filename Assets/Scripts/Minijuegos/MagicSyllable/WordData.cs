using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WordData", menuName = "SyllableGame/WordData")]
public class WordData : ScriptableObject
{
    public string fullWord;
    public string[] syllables;
    public string correctSyllable; 
    public Sprite image;
}