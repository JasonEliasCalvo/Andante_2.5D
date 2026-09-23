using UnityEngine;

public class ActionSimulationRunner : MonoBehaviour
{
    [Header("Referencias de Unity")]
    private FighterEntity fighterEntity;

    private ActionSimulation simulation;
    public ActionData CurrentAction => simulation != null ? simulation.CurrentAction : null;
    public bool IsActive => simulation != null && simulation.IsActive;

    private void Awake()
    {

        fighterEntity = GetComponent<FighterEntity>();

        // 1. Creamos la simulación (le pasamos el gameObject para los efectos actuales)
        simulation = new ActionSimulation(gameObject);

        simulation.OnActionStarted += HandleActionStarted;
        simulation.OnActionEnded += HandleActionEnded;
    }

    private void Start()
    {
        // Al nacer, nos suscribimos al Reloj Global
        if (CombatTickManager.Instance != null)
            CombatTickManager.Instance.RegisterRunner(this);
        else
            Debug.LogWarning($"[{gameObject.name}] No hay CombatTickManager en la escena.");
    }

    private void OnDestroy()
    {
        // Al morir, nos damos de baja
        if (CombatTickManager.Instance != null)
            CombatTickManager.Instance.UnregisterRunner(this);

        if (simulation != null)
        {
            simulation.OnActionStarted -= HandleActionStarted;
            simulation.OnActionEnded -= HandleActionEnded;
        }
    }

    // ------------------------------------------------------------------
    public void TickLogic()
    {
        simulation.AdvanceFrame();
    }

    public void TickVisuals()
    {
        // Actualizamos los parámetros del BlendTree de locomoción
        if (fighterEntity.Locomotion != null && fighterEntity.Visuals != null)
        {
            fighterEntity.Visuals.UpdateLocomotionState(fighterEntity.Locomotion.IsGrounded, fighterEntity.Locomotion.SubPhase == LocomotionSubPhase.Falling);
        }
    }

    // ------------------------------------------------------------------
    public bool RequestAction(ActionData action, ActionContext context)
    {
        return simulation.StartAction(action, context);
    }

    public void InterruptAction()
    {
        if (simulation != null && simulation.IsActive) simulation.InterruptAction();
    }

    private void HandleActionStarted(ActionData action)
    {
        fighterEntity.Movement?.SetMovementLock(MovementLockSource.Action, action.lockHorizontalMovement);
        fighterEntity.Visuals?.PlayAction(action.animation, 0f);
    }

    private void HandleActionEnded()
    {
        fighterEntity.Movement?.SetMovementLock(MovementLockSource.Action, false);
        fighterEntity.Visuals?.StopAction(0.1f);
    }

}