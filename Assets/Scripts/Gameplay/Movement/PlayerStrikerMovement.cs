using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public sealed class PlayerStrikerMovement : StrikerMovement
{
    [SerializeField] private PlayerInputReader inputReader;

    private Vector2 currentMoveDirection;
    private AbilityActivationTrigger pendingActivationTriggers;

    private void Reset()
    {
        if (!inputReader)
            inputReader = GetComponent<PlayerInputReader>();
    }

    public bool Initialize(PlayerControlScheme controlScheme, StrikerSetupContext setupContext)
    {
        if (!inputReader)
            inputReader = GetComponent<PlayerInputReader>();

        if (!inputReader)
        {
            Debug.LogError($"{nameof(PlayerStrikerMovement)} on {name} requires a {nameof(PlayerInputReader)} component.", this);
            return false;
        }

        DisconnectInputEvents();

        currentMoveDirection = Vector2.zero;
        pendingActivationTriggers = AbilityActivationTrigger.None;

        inputReader.Initialize(controlScheme);
        inputReader.MoveInputChanged += HandleMoveInputChanged;
        inputReader.AbilityActivationPressed += HandleAbilityActivationPressed;

        currentMoveDirection = inputReader.CurrentMoveInput;

        if (!base.InitializeStrikerMovement(setupContext))
        {
            DisconnectInputEvents();
            return false;
        }

        UpdateMovementLoopState();
        return true;
    }

    private void FixedUpdate()
    {
        if (!CanMoveThisFrame()) return;

        var command = new MovementCommand(currentMoveDirection, pendingActivationTriggers);
        pendingActivationTriggers = AbilityActivationTrigger.None;

        ExecuteMovementStep(command);
        UpdateMovementLoopState();
    }

    private void OnDestroy()
    {
        DisconnectInputEvents();
    }

    protected override void UpdateMovementLoopState()
    {
        if (!IsInitialized)
        {
            enabled = false;
            return;
        }

        if (!IsMovementAllowed)
        {
            enabled = false;
            return;
        }

        var hasMoveInput = currentMoveDirection.sqrMagnitude > 0.0001f;
        var hasAbilityActivity = pendingActivationTriggers != AbilityActivationTrigger.None || HasActiveAbilityWork;

        enabled = hasMoveInput || hasAbilityActivity;
    }

    protected override void HandleMovementStopped()
    {
        pendingActivationTriggers = AbilityActivationTrigger.None;
    }

    protected override void HandleMovementReset()
    {
        pendingActivationTriggers = AbilityActivationTrigger.None;
    }

    private void HandleMoveInputChanged(Vector2 moveDirection)
    {
        currentMoveDirection = moveDirection;

        if (!IsMovementAllowed) return;

        if (moveDirection.sqrMagnitude <= 0.0001f &&
            pendingActivationTriggers == AbilityActivationTrigger.None &&
            !HasActiveAbilityWork)
            StopMovement();

        UpdateMovementLoopState();
    }

    private void HandleAbilityActivationPressed(AbilityActivationTrigger activationTrigger)
    {
        if (!IsMovementAllowed) return;

        pendingActivationTriggers |= activationTrigger;
        UpdateMovementLoopState();
    }

    private void DisconnectInputEvents()
    {
        if (!inputReader) return;

        inputReader.MoveInputChanged -= HandleMoveInputChanged;
        inputReader.AbilityActivationPressed -= HandleAbilityActivationPressed;
        inputReader.Shutdown();
    }

    private bool CanMoveThisFrame()
    {
        if (IsMovementAllowed) return true;

        StopMovement();
        return false;
    }
}
