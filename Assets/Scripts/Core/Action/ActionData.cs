using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Combat/Action")]
public class ActionData : ScriptableObject
{
    [Header("Identidad")]
    public string actionName;
    public ActionType actionType;
    public int priority = 1;

    [Header("Input")]
    public InputCommandType inputType;
    public InputDirection allowedDirections;

    [Header("Duración Total (60 FPS)")]
    public int totalFrames = 30;

    [Header("Animation")]
    public AnimationClip animation;

    [Header("Movimiento")]
    public bool lockHorizontalMovement = false;

    [Header("Cancel Windows")]
    public int cancelStartFrame = 0;
    public bool canBeCanceledOnHit = true;

    [Header("Condiciones para Ejecutar")]
    [SerializeReference, SubclassSelector] public List<ActionCondition> conditions = new List<ActionCondition>();

    [Header("Timeline de Efectos")]
    public List<ActionTimelineEntry> timeline = new List<ActionTimelineEntry>();
}

public enum ActionType
{
    NormalAttack,
    ChargeAttack,
    Displacement,
    Other
}