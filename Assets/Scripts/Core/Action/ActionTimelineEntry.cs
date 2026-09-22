using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActionTimelineEntry
{
    [Tooltip("Frame exacto en el que inicia el efecto (a 60 FPS)")]
    public int startFrame = 0;

    [Tooltip("Duración en frames. 1 = Instantáneo (ej. Salto). >1 = Activo por N frames (ej. Hitbox, I-Frames)")]
    public int durationFrames = 1;

    [SerializeReference, SubclassSelector] public ActionEffect effect;

    public int EndFrame => startFrame + durationFrames;
}