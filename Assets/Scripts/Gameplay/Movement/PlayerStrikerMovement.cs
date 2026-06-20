using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public sealed class PlayerStrikerMovement : StrikerMovement
{
    [SerializeField] private PlayerInputReader inputReader;

    private Vector2 currentMoveDirection;

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

        inputReader.Initialize(controlScheme);
        inputReader.MoveInputChanged += HandleMoveInputChanged;

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

        var command = new MovementCommand(currentMoveDirection, false);
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
        var hasDashActivity = IsDashActive;

        enabled = hasMoveInput || hasDashActivity;
    }

    protected override void HandleMovementStopped()
    {
    }

    protected override void HandleMovementReset()
    {
        base.HandleMovementReset();
    }

    private void HandleMoveInputChanged(Vector2 moveDirection)
    {
        currentMoveDirection = moveDirection;
        SetCurrentMoveDirection(currentMoveDirection);

        if (!IsMovementAllowed) return;

        if (moveDirection.sqrMagnitude <= 0.0001f && !IsDashActive)
            StopMovement();

        UpdateMovementLoopState();
    }

    private void DisconnectInputEvents()
    {
        if (!inputReader) return;

        inputReader.MoveInputChanged -= HandleMoveInputChanged;
        inputReader.Shutdown();
    }

    private bool CanMoveThisFrame()
    {
        if (IsMovementAllowed) return true;

        StopMovement();
        return false;
    }
}
