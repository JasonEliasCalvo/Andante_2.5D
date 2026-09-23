using System.Collections.Generic;
using UnityEngine;

public class CombatTickManager : MonoBehaviour
{
    public static CombatTickManager Instance { get; private set; }

    [Header("Configuración de Simulación")]
    [SerializeField] private int targetFPS = 60;
    [SerializeField] private int maxTicksPerFrame = 5;

    private float timePerTick;
    private float accumulator = 0f;

    // Aquí guardamos a todos los personajes (Jugador y Enemigos)
    private List<ActionSimulationRunner> registeredRunners = new List<ActionSimulationRunner>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        timePerTick = 1f / targetFPS;
    }

    public void RegisterRunner(ActionSimulationRunner runner)
    {
        if (!registeredRunners.Contains(runner)) registeredRunners.Add(runner);
    }

    public void UnregisterRunner(ActionSimulationRunner runner)
    {
        if (registeredRunners.Contains(runner)) registeredRunners.Remove(runner);
    }

    private void Update()
    {
        accumulator += Time.deltaTime;
        int ticksProcessed = 0;

        while (accumulator >= timePerTick && ticksProcessed < maxTicksPerFrame)
        {
            // 1. LÓGICA: Todos los personajes avanzan su mente a la vez
            for (int i = 0; i < registeredRunners.Count; i++)
            {
                registeredRunners[i].TickLogic();
            }

            accumulator -= timePerTick;
            ticksProcessed++;
        }

        if (ticksProcessed == maxTicksPerFrame) accumulator = 0f;

        // 2. GRÁFICOS: Una vez que todos decidieron qué hacer, actualizamos sus cuerpos
        for (int i = 0; i < registeredRunners.Count; i++)
        {
            registeredRunners[i].TickVisuals();
        }
    }
}