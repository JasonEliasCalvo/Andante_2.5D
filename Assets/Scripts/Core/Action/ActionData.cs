using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Combat/Action")]
public class ActionData : ScriptableObject
{
    [Header("Identidad")]
    public string actionName;
    public ActionType actionType;

    [Header("Input")]
    public InputCommandType inputType;
    public InputDirection allowedDirections;

    [Header("Animation")]
    public string animationStateName;
    public float duration = 0.1f;

    [Header("Movimiento")]
    public bool lockHorizontalMovement = false;

    [Header("Condiciones para Ejecutar")]
    [SerializeReference, SubclassSelector] public List<ActionCondition> conditions = new List<ActionCondition>();

    [Header("Efectos al Ejecutar")]
    [SerializeReference, SubclassSelector] public List<ActionEffect> effects = new List<ActionEffect>();
}

public enum ActionType
{
    Attack,
    Displacement,
    Special,
    Other
}