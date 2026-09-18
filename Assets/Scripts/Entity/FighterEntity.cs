using UnityEngine;
using UnityEngine.Events;

public abstract class FighterEntity : MonoBehaviour, IDamageable
{
    [Header("Core Components")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected HealthComponent health;
    [SerializeField] protected AudioSource audioSource;

    [SerializeField] protected CharacterMovement movement;
    [SerializeField] protected LocomotionSystem locomotionSystem;
    [SerializeField] protected ActionSystem actionSystem;
    [SerializeField] protected ActionResolver actionResolver;
    [SerializeField] protected ReactionSystem reactionSystem;
    [SerializeField] protected FighterAnimator fighterAnimator;

    [Space(10)]
    public UnityEvent onDeathEnd;

    [Header("Input")]
    [SerializeField] protected CharacterInputSource inputSource;

    [Header("Combat System")]
    public CombatHitbox rightHandBox; // 0
    public CombatHitbox leftHandBox; // 1
    public CombatHitbox rightFootBox; // 2
    public CombatHitbox leftFootBox; // 3
    public CombatHitbox weaponBox; // 4

    [Header("MoveSet")]
    public MoveSet moveSet;
    public MoveSet defaultMoveSet;

    public bool IsInvulnerable { get; set; }

    public Vector3 MovementInput =>
    inputSource != null
        ? inputSource.GetMovementDirection()
        : Vector3.zero;

    public CharacterMovement Movement => movement;
    public LocomotionSystem Locomotion => locomotionSystem;

    public ActionSystem Actions => actionSystem;

    public ReactionSystem Reactions => reactionSystem;

    public FighterAnimator FighterAnimator => fighterAnimator;

    public Animator Animator => animator;


    protected virtual void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        health = GetComponent<HealthComponent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        inputSource = GetComponent<CharacterInputSource>();
        locomotionSystem = GetComponent<LocomotionSystem>();
        actionSystem = GetComponent<ActionSystem>();
        reactionSystem = GetComponent<ReactionSystem>();
        actionResolver = new ActionResolver();
        fighterAnimator = GetComponent<FighterAnimator>();

        if (movement != null && inputSource != null)
            movement.SetInputSource(inputSource);

        if (health != null)
            health.OnDeath += HandleDeath;
    }

    protected virtual void Start()
    {
        if (moveSet == null)
            moveSet = defaultMoveSet;
    }

    // --- DAMAGE ---
    public virtual void TakeDamage(float amount, float hitStun)
    {
        if (IsInvulnerable) return;

        health.ApplyDamage(amount);

        if (reactionSystem.CurrentReaction == ReactionSystem.ReactionState.Dead)
        {
            return;
        }

        if (health.CurrentHealth <= 0f)
            return;

        reactionSystem.StartHit(
            hitStun
        );
    }

    public void Heal(float amount)
    {
        health.Heal(amount);
    }

    private void HandleDeath()
    {
        reactionSystem.StartDeath();
    }

    public void InstantDeath()
    {
        if (reactionSystem.CurrentReaction == ReactionSystem.ReactionState.Dead)
            return;

        health.ApplyDamage( health.CurrentHealth);
    }

    // --- ACTIONS ---
    public bool ExecuteAction(ActionData action, ActionContext context)
    {
        return actionSystem.StartAction(action, context);
    }

    // --- HITBOXES ---
    public void AnimEvent_OpenHitbox(int limbIndex)
    {
        ActionData action = actionSystem.CurrentAction;
        if (action == null) return;

        DamageEffect damageEffect = null;
        for (int i = 0; i < action.effects.Count; i++)
        {
            if (action.effects[i] is DamageEffect effect)
            {
                damageEffect = effect;
                break;
            }
        }

        if (damageEffect == null || damageEffect.attackData == null) return;

        CombatHitbox target = GetHitbox(limbIndex);
        if (target == null) return;

        target.EnableHitbox(damageEffect.attackData, action);
    }

    public void AnimEvent_CloseHitbox(int limbIndex)
    {
        CombatHitbox targetBox = GetHitbox(limbIndex);

        targetBox?.DisableHitbox();
    }

    private CombatHitbox GetHitbox( int limbIndex)
    {
        return limbIndex switch
        {
            0 => rightHandBox,
            1 => leftHandBox,
            2 => rightFootBox,
            3 => leftFootBox,
            4 => weaponBox,
            _ => null
        };
    }

    public void AnimEvent_PlaySwingSound()
    {
        ActionData action = actionSystem.CurrentAction;

        if (action == null)
            return;
    }

    public void AnimEvent_SpawnAttackParticle(int limbIndex)
    {
        ActionData action = actionSystem.CurrentAction;
        Transform targetTransform = transform;

        switch (limbIndex)
        {
            case 0: if (rightHandBox != null) targetTransform = rightHandBox.transform; break;
            case 1: if (leftHandBox != null) targetTransform = leftHandBox.transform; break;
            case 2: if (rightFootBox != null) targetTransform = rightFootBox.transform; break;
            case 3: if (leftFootBox != null) targetTransform = leftFootBox.transform; break;
            case 4: if (weaponBox != null) targetTransform = weaponBox.transform; break;
        }
    }

    public void AnimEvent_PlayAudioDirect(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
