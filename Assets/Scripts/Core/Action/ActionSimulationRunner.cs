using UnityEngine;

public class ActionSimulationRunner : MonoBehaviour
{
    [Header("Configuración de Simulación")]
    [SerializeField] private int targetFPS = 60;
    [SerializeField] private int maxTicksPerFrame = 5;

    [Header("Referencias de Unity")]
    private CharacterMovement movement;
    private PlayableActionSystem playableActionSystem;

    private ActionSimulation simulation;
    private float timePerTick;
    private float accumulator = 0f;

    public ActionData CurrentAction => simulation != null ? simulation.CurrentAction : null;

    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        playableActionSystem = GetComponent<PlayableActionSystem>();

        // 1. Creamos la simulación (le pasamos el gameObject para los efectos actuales)
        simulation = new ActionSimulation(gameObject);
        timePerTick = 1f / targetFPS;

        // 2. Nos suscribimos a los eventos del Cerebro
        simulation.OnActionStarted += HandleActionStarted;
        simulation.OnActionEnded += HandleActionEnded;
    }

    private void OnDestroy()
    {
        // Limpiamos memoria
        if (simulation != null)
        {
            simulation.OnActionStarted -= HandleActionStarted;
            simulation.OnActionEnded -= HandleActionEnded;
        }
    }

    private void Update()
    {
        accumulator += Time.deltaTime;
        int ticksProcessed = 0;

        while (accumulator >= timePerTick && ticksProcessed < maxTicksPerFrame)
        {
            simulation.AdvanceFrame();
            //locomotionSystem.Tick();

            accumulator -= timePerTick;
            ticksProcessed++;
        }

        if (ticksProcessed == maxTicksPerFrame) accumulator = 0f;
    }

    public bool RequestAction(ActionData action, ActionContext context)
    {
        return simulation.StartAction(action, context);
    }

    private void HandleActionStarted(ActionData action)
    {
        // El cerebro dijo que la acción empezó. Unity hace el trabajo visual.
        movement?.SetMovementLock(MovementLockSource.Action, action.lockHorizontalMovement);
        playableActionSystem?.PlayAction(action.animation, 0f);
    }

    private void HandleActionEnded()
    {
        // El cerebro dijo que la acción terminó. Liberamos a Unity.
        movement?.SetMovementLock(MovementLockSource.Action, false);
        playableActionSystem?.StopAction(0.1f);
    }

    public void InterruptAction()
    {
        if (simulation != null && simulation.IsActive)
        {
            simulation.InterruptAction();
        }
    }
}