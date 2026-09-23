using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class PlayableActionSystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float defaultCrossfadeDuration = 0.008f;

    private PlayableGraph graph;
    private AnimationMixerPlayable mixer;
    private AnimatorControllerPlayable locomotionPlayable;

    // Slot 0: Animator Controller Base (Idle / Walk / Run / Fall / Land)
    // Slot 1: Acción Genérica A (Ataque, Dash, Salto, Roll, Hurt)
    // Slot 2: Acción Genérica B (Para encadenar/cancelar hacia la siguiente acción sin cortes)
    private AnimationClipPlayable[] actionSlots = new AnimationClipPlayable[2];

    private int activeSlotIndex = -1; // -1 = Locomoción, 1 = Slot A, 2 = Slot B
    private float transitionSpeed;
    private float[] targetWeights = new float[3];
    private float[] currentWeights = new float[3];

    private Animator animator;
    private float hitStopTimer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        InitializeGraph();
    }

    private void InitializeGraph()
    {
        // 1. Crear el PlayableGraph
        graph = PlayableGraph.Create($"{gameObject.name}_CombatGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        // 2. Mixer de 3 entradas
        mixer = AnimationMixerPlayable.Create(graph, 3);
        // 3. Conectar el Animator Controller original a la entrada 0 (Locomoción/Caída/Land)
        if (animator.runtimeAnimatorController != null)
        {
            locomotionPlayable = AnimatorControllerPlayable.Create(graph, animator.runtimeAnimatorController);
            graph.Connect(locomotionPlayable, 0, mixer, 0);
        }

        var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        output.SetSourcePlayable(mixer);

        currentWeights[0] = 1f;
        targetWeights[0] = 1f;
        mixer.SetInputWeight(0, 1f);

        graph.Play();
    }

    /// <summary>
    /// Ejecuta un clip de acción INMEDIATAMENTE sin lag de frame.
    /// </summary>
    public void PlayAction(AnimationClip clip, float fadeDuration = -1f)
    {
        if (clip == null) return;

        Debug.Log($"[PlayableActionSystem] PlayAction: {clip.name} (Fade: {fadeDuration}s)");

        float fade = (fadeDuration >= 0) ? fadeDuration : defaultCrossfadeDuration;
        transitionSpeed = (fade > 0f) ? (1f / fade) : 10000f; // Evita división por cero si fade es 0

        // Alternar entre Slot 1 y Slot 2 (Ping-Pong)
        int nextSlot = (activeSlotIndex == 1) ? 2 : 1;
        int oldSlot = (activeSlotIndex == 1) ? 1 : 2;
        int slotArrayIndex = nextSlot - 1;

        // Limpiar el slot que vamos a reutilizar
        if (actionSlots[slotArrayIndex].IsValid())
        {
            graph.Disconnect(mixer, nextSlot);
            actionSlots[slotArrayIndex].Destroy();
        }

        // Crear, reiniciar tiempo y conectar la nueva animación
        actionSlots[slotArrayIndex] = AnimationClipPlayable.Create(graph, clip);
        actionSlots[slotArrayIndex].SetTime(0);
        actionSlots[slotArrayIndex].SetSpeed(1);
        graph.Connect(actionSlots[slotArrayIndex], 0, mixer, nextSlot);

        // Definir pesos objetivo
        targetWeights[0] = 0f;        // Desactivar Locomoción
        targetWeights[nextSlot] = 1f;  // Activar nuevo ataque
        targetWeights[oldSlot] = 0f;   // Desactivar ataque anterior

        // Si la transición es instantánea (0s), aplicar pesos en el acto
        if (fade <= 0f)
        {
            for (int i = 0; i < 3; i++)
            {
                currentWeights[i] = targetWeights[i];
                mixer.SetInputWeight(i, currentWeights[i]);
            }
        }

        activeSlotIndex = nextSlot;
    }


    /// <summary>
    /// Vuelve a la locomoción (Animator Controller) de forma suave.
    /// </summary>
    public void StopAction(float fadeDuration = -1f)
    {
        if (activeSlotIndex == -1) return;

        Debug.Log($"[PlayableActionSystem] StopAction (Fade: {fadeDuration}s)");

        float fade = (fadeDuration >= 0) ? fadeDuration : defaultCrossfadeDuration;
        transitionSpeed = (fade > 0f) ? (1f / fade) : 10000f;

        targetWeights[0] = 1f; // Volver a Locomoción
        targetWeights[1] = 0f;
        targetWeights[2] = 0f;

        activeSlotIndex = -1;
    }

    private void Update()
    {
        HandleHitStop();
        UpdateWeights();
        UpdateLocomotionState(GetComponent<LocomotionSystem>());
    }

    private void UpdateWeights()
    {
        // Mezclado continuo de pesos (Crossfade en tiempo real)
        for (int i = 0; i < 3; i++)
        {
            if (!Mathf.Approximately(currentWeights[i], targetWeights[i]))
            {
                currentWeights[i] = Mathf.MoveTowards(currentWeights[i], targetWeights[i], transitionSpeed * Time.deltaTime);
                mixer.SetInputWeight(i, currentWeights[i]);
            }
        }
    }

    public void UpdateLocomotionState(LocomotionSystem locomotion)
    {
        if (activeSlotIndex == -1)
        {
            locomotionPlayable.SetBool("IsFalling", locomotion.SubPhase == LocomotionSubPhase.Falling);
            locomotionPlayable.SetBool("IsGrounded", locomotion.IsGrounded);
        }
    }

    public void ApplyHitStop(float duration)
    {
        if (duration <= 0f) return;
        hitStopTimer = duration;

        if (activeSlotIndex != -1)
        {
            int slotArrayIndex = activeSlotIndex - 1;
            if (actionSlots[slotArrayIndex].IsValid())
            {
                actionSlots[slotArrayIndex].SetSpeed(0); // Pausa el clip activo
            }
        }
    }

    private void HandleHitStop()
    {
        if (hitStopTimer > 0f)
        {
            hitStopTimer -= Time.deltaTime;
            if (hitStopTimer <= 0f)
            {
                hitStopTimer = 0f;
                if (activeSlotIndex != -1)
                {
                    int slotArrayIndex = activeSlotIndex - 1;
                    if (actionSlots[slotArrayIndex].IsValid())
                    {
                        actionSlots[slotArrayIndex].SetSpeed(1); // Reanuda la animación
                    }
                }
            }
        }
    }

    #region Proxies para el Animator Controller
    public void SetFloat(string name, float value) => locomotionPlayable.SetFloat(name, value);
    public void SetBool(string name, bool value) => locomotionPlayable.SetBool(name, value);
    public void SetInteger(string name, int value) => locomotionPlayable.SetInteger(name, value);
    public void SetTrigger(string name) => locomotionPlayable.SetTrigger(name);
    #endregion

    #region Helpers
    public float GetActionNormalizedTime()
    {
        if (activeSlotIndex == -1) return 0f;
        int slotIndex = activeSlotIndex - 1;

        if (!actionSlots[slotIndex].IsValid()) return 0f;

        var clip = actionSlots[slotIndex].GetAnimationClip();
        if (clip == null || clip.length <= 0f) return 0f;

        return (float)(actionSlots[slotIndex].GetTime() / clip.length);
    }

    public bool IsPlayingAction()
    {
        return activeSlotIndex != -1 && targetWeights[0] < 0.5f;
    }
    #endregion

    private void OnDestroy()
    {
        if (graph.IsValid())
        {
            graph.Destroy();
        }
    }
}