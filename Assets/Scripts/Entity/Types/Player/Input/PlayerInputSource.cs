using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSource : CharacterInputSource
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private PlayerControls controls;

    private Vector2 rawMovementInput;
    private Vector2 rawLookInput;

    private readonly Queue<InputCommand> commandQueue = new();

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Move.performed += OnMovePerformed;
        controls.Gameplay.Move.canceled += OnMoveCanceled;

        controls.Gameplay.Look.performed += OnLookPerformed;
        controls.Gameplay.Look.canceled += OnLookCanceled;

        controls.Gameplay.Attack.performed += OnAttackPerformed;
        controls.Gameplay.Dash.performed += OnDashPerformed;
        controls.Gameplay.Jump.performed += OnJumpPerformed;
        controls.Gameplay.Interact.performed += OnInteractPerformed;
    }

    private void OnEnable()
    {
        controls?.Enable();
    }

    private void OnDisable()
    {
        controls?.Disable();
    }

    private void OnDestroy()
    {
        controls.Gameplay.Move.performed -= OnMovePerformed;
        controls.Gameplay.Move.canceled -= OnMoveCanceled;

        controls.Gameplay.Attack.performed -= OnAttackPerformed;
        controls.Gameplay.Dash.performed -= OnDashPerformed;
        controls.Gameplay.Jump.performed -= OnJumpPerformed;
        controls.Gameplay.Interact.performed -= OnInteractPerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        rawMovementInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        rawMovementInput = Vector2.zero;
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        rawLookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        rawLookInput = Vector2.zero;
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        EnqueueCommand(InputCommandType.Attack);
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        EnqueueCommand(InputCommandType.Dash);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        EnqueueCommand(InputCommandType.Jump);
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        EnqueueCommand(InputCommandType.Interact);
    }

    private void EnqueueCommand(InputCommandType type)
    {
        Vector2 movement = rawMovementInput;

        InputDirection direction = GetInputDirection(movement);

        InputCommand command = new InputCommand(
            type,
            direction,
            movement,
            Time.time
        );

        commandQueue.Enqueue(command);
    }

    private InputDirection GetInputDirection(Vector2 input)
    {
        const float threshold = 0.25f;

        if (input.sqrMagnitude < threshold * threshold)
            return InputDirection.None;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return input.x > 0
                ? InputDirection.Right
                : InputDirection.Left;

        return input.y > 0
            ? InputDirection.Forward
            : InputDirection.Back;
    }

    public Vector2 GetLookInput()
    {
        return rawLookInput;
    }

    public override bool TryConsumeCommand(out InputCommand command)
    {
        if (commandQueue.Count == 0)
        {
            command = default;
            return false;
        }

        command = commandQueue.Dequeue();
        return true;
    }

    public override Vector3 GetMovementDirection()
    {
        if (rawMovementInput.sqrMagnitude < 0.01f)
            return Vector3.zero;

        Vector3 forward;
        Vector3 right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();
        }
        else
        {
            forward = Vector3.forward;
            right = Vector3.right;
        }

        Vector3 movement =
            forward * rawMovementInput.y +
            right * rawMovementInput.x;

        return movement.sqrMagnitude > 1f
            ? movement.normalized
            : movement;
    }

    public Vector2 RawMovementInput => rawMovementInput;
}