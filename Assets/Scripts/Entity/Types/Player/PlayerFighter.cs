using UnityEngine;

public class PlayerFighter : FighterEntity
{
    private PlayerInputSource playerInput;

    [Header("Input Buffer")]
    [SerializeField] private float inputBufferDuration = 0.25f;

    private InputBuffer inputBuffer;

    [Header("Dash")]
    [SerializeField] private float dashCooldown = 0.8f;
    private float dashCooldownTimer;

    protected override void Awake()
    {
        base.Awake();

        if (playerInput == null)
            playerInput = GetComponent<PlayerInputSource>();

        if (playerInput == null)
        {
            Debug.LogError(
                $"{name}: PlayerFighter necesita PlayerInputSource.",
                this
            );

            return;
        }

        Movement.SetInputSource( playerInput );
        inputBuffer = new InputBuffer(inputBufferDuration);
    }

    protected override void Start()
    {
        base.Start();

        if (GameManager.instance != null)
        {
            GameManager.instance.InitialGameStart += HandleGameStart;
            GameManager.instance.InitialGameEnd += HandleGameEnd;
        }
    }

    protected void Update()
    {
        ReadInputCommands();

        inputBuffer.Update();

        ProcessActionInput();

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;
    }

    private void ReadInputCommands()
    {
        if (playerInput == null)
            return;

        while (playerInput.TryConsumeCommand(out InputCommand command))
        {
            inputBuffer.Add(command);

            Debug.Log(
                $"INPUT BUFFER → {command.type} | " +
                $"Direction: {command.direction}"
            );
        }
    }

    private void ProcessActionInput()
    {
        if (Reactions.IsReacting)
            return;

        if (Actions.IsActive)
        {
            // Más adelante aquí tendremos
            // cancel windows.
            return;
        }

        if (!inputBuffer.TryPeek(out InputCommand command))
            return;

        ActionContext context =
            new ActionContext(Locomotion, MovementInput, Actions.CurrentAction, false);

        ActionData action =
            actionResolver.Resolve( command, context, moveSet);

        if (action == null)
            return;

        inputBuffer.TryConsume( out command);

        Debug.Log(
            $"ACTION RESOLVED → " +
            $"{action.actionName}"
        );

        ExecuteAction(action, context);

        // Interact posteriormente.
    }

    public bool TryConsumeInputCommand(out InputCommand command)
    {
        return inputBuffer.TryConsume(out command);
    }

    public bool TryPeekInputCommand(out InputCommand command)
    {
        return inputBuffer.TryPeek(out command);
    }

    public void ClearInputBuffer()
    {
        inputBuffer.Clear();
    }

    public void StartDashCooldown()
    {
        dashCooldownTimer = dashCooldown;
    }
    private void HandleGameStart()
    {
        Movement.SetHorizontalMovementEnabled(true);
    }

    private void HandleGameEnd()
    {
        Movement.StopHorizontalMovement();
        Movement.SetHorizontalMovementEnabled(false);
    }
}