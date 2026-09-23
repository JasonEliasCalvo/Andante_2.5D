using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionSimulation
{
    public int SimulationFrame { get; private set; }

    public int ActionFrame { get; private set; }
    public ActionData CurrentAction { get; private set; }
    public ActionContext CurrentContext { get; private set; }
    public bool IsActive => CurrentAction != null;
    public event Action<ActionData> OnActionStarted;
    public event Action OnActionEnded;

    private List<ActionTimelineEntry> activeEntries = new List<ActionTimelineEntry>();
    private GameObject userGameObject;

    public ActionSimulation(GameObject user)
    {
        userGameObject = user;
    }

    public void AdvanceFrame()
    {
        // 1. El tiempo del mundo SIEMPRE avanza
        SimulationFrame++;

        // 2. Si no hay acción, procesaríamos locomoción/física, pero no avanzamos ActionFrame
        if (!IsActive) return;

        // 3. Evaluar la línea temporal en el frame actual de la acción
        ProcessTimeline();

        // 4. Avanzar el frame local para el siguiente tick
        ActionFrame++;

        // 5. Terminar la acción si completó su duración
        if (ActionFrame >= CurrentAction.totalFrames)
        {
            EndAction();
        }
    }

    private void ProcessTimeline()
    {
        string debugLog = $"Simulation Frame: {SimulationFrame}\nAction: {CurrentAction.actionName}\nAction Frame: {ActionFrame}\n\nTimeline:\n";
        bool hasEffectsThisFrame = false;

        // A. Evaluar Inicios (OnStart)
        foreach (var entry in CurrentAction.timeline)
        {
            if (ActionFrame == entry.startFrame)
            {
                // TODO: Desacoplar GameObject/Context en el futuro
                entry.effect?.OnStart(userGameObject, CurrentContext);
                activeEntries.Add(entry);

                debugLog += $"{entry.effect.GetType().Name} -> START\n";
                hasEffectsThisFrame = true;
            }
        }

        // B. Evaluar Mantenimiento (OnUpdate)
        foreach (var entry in activeEntries)
        {
            if (ActionFrame > entry.startFrame && ActionFrame < entry.EndFrame)
            {
                int activeFrame = ActionFrame - entry.startFrame;
                entry.effect?.OnUpdate(userGameObject, CurrentContext, activeFrame);
                debugLog += $"{entry.effect.GetType().Name} -> ACTIVE\n";
                hasEffectsThisFrame = true;
            }
        }

        // C. Evaluar Finales (OnEnd)
        for (int i = activeEntries.Count - 1; i >= 0; i--)
        {
            var entry = activeEntries[i];
            if (ActionFrame == entry.EndFrame)
            {
                entry.effect?.OnEnd(userGameObject, CurrentContext);
                activeEntries.RemoveAt(i);
                debugLog += $"{entry.effect.GetType().Name} -> END\n";
                hasEffectsThisFrame = true;
            }
        }

        // imprimir el log solo si pasó algo
        if (hasEffectsThisFrame) Debug.Log(debugLog);
    }

    public bool StartAction(ActionData action, ActionContext context)
    {
        if (action == null) return false;

        if (IsActive)
        {
            if (!CanInterruptCurrentAction(action)) return false;
            InterruptAction();
        }

        CurrentAction = action;
        CurrentContext = context;
        activeEntries.Clear();
        ActionFrame = 0;

        Debug.Log($"<color=cyan>Simulación → START ACTION ({action.actionName}) | Priority: {action.priority}</color>");

        // Disparamos el evento para que Unity (el Runner) lo escuche y ponga la animación
        OnActionStarted?.Invoke(action);

        return true;
    }
    public void EndAction() => CleanupAction();
    public void InterruptAction() => CleanupAction();

    private void CleanupAction()
    {
        for (int i = 0; i < activeEntries.Count; i++)
        {
            activeEntries[i].effect?.OnEnd(userGameObject, CurrentContext);
        }

        CurrentAction = null;
        CurrentContext = default;
        activeEntries.Clear();
        ActionFrame = 0;

        OnActionEnded?.Invoke();
    }

    private bool CanInterruptCurrentAction(ActionData newAction)
    {
        if (CurrentAction == null) return true;
        if (newAction.priority > CurrentAction.priority) return true;
        if (CurrentAction.canBeCanceledOnHit && ActionFrame >= CurrentAction.cancelStartFrame) return true;

        return false;
    }
}
