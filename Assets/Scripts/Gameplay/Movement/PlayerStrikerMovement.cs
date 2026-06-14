using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public sealed class PlayerStrikerMovement : StrikerMovement
{
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private bool useLegacyDirectDash;

    private Vector2 currentMoveDirection;
    private bool dashRequested;

    private void Reset()
    {
        if (!inputReader)
            inputReader = GetComponent<PlayerInputReader>();
    }

    public bool Initialize(PlayerControlScheme controlScheme)
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
        SetCurrentMoveDirection(currentMoveDirection);
        dashRequested = false;

        inputReader.Initialize(controlScheme);
        inputReader.MoveInputChanged += HandleMoveInputChanged;
        inputReader.DashPressed += HandleDashPressed;

        currentMoveDirection = inputReader.CurrentMoveInput;
        SetCurrentMoveDirection(currentMoveDirection);

        if (!base.InitializeStrikerMovement())
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

        var command = new MovementCommand(currentMoveDirection, dashRequested);
        dashRequested = false;

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
        var hasDashActivity = dashRequested || IsDashActive;

        enabled = hasMoveInput || hasDashActivity;
    }

    protected override void HandleMovementStopped()
    {
        dashRequested = false;
    }

    protected override void HandleMovementReset()
    {
        base.HandleMovementReset();
        dashRequested = false;
    }

    private void HandleMoveInputChanged(Vector2 moveDirection)
    {
        currentMoveDirection = moveDirection;
        SetCurrentMoveDirection(currentMoveDirection);

        if (!IsMovementAllowed) return;

        if (moveDirection.sqrMagnitude <= 0.0001f && !dashRequested && !IsDashActive)
            StopMovement();

        UpdateMovementLoopState();
    }

    private void HandleDashPressed()
    {
        if (!useLegacyDirectDash) return;
        if (!IsMovementAllowed) return;

        dashRequested = true;
        UpdateMovementLoopState();
    }

    private void DisconnectInputEvents()
    {
        if (!inputReader) return;

        inputReader.MoveInputChanged -= HandleMoveInputChanged;
        inputReader.DashPressed -= HandleDashPressed;
        inputReader.Shutdown();
    }

    private bool CanMoveThisFrame()
    {
        if (IsMovementAllowed) return true;

        StopMovement();
        return false;
    }
}
