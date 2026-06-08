using UnityEngine;

[RequireComponent(typeof(PlayerInputCommandSource))]
public sealed class PlayerStrikerMovement : StrikerMovement
{
    [SerializeField] private PlayerInputCommandSource inputCommandSource;

    private Vector2 currentMoveDirection;
    private bool dashRequested;

    private void Reset()
    {
        if (!inputCommandSource)
            inputCommandSource = GetComponent<PlayerInputCommandSource>();
    }

    public bool Initialize(PlayerControlScheme controlScheme, BoxCollider2D strikerBoundsCollider)
    {
        if (!inputCommandSource)
            inputCommandSource = GetComponent<PlayerInputCommandSource>();

        if (!inputCommandSource)
        {
            Debug.LogError($"{nameof(PlayerStrikerMovement)} on {name} requires a {nameof(PlayerInputCommandSource)} component.", this);
            return false;
        }

        DisconnectInputEvents();

        currentMoveDirection = Vector2.zero;
        dashRequested = false;

        inputCommandSource.Initialize(controlScheme);
        inputCommandSource.MoveInputChanged += HandleMoveInputChanged;
        inputCommandSource.DashInputChanged += HandleDashInputChanged;

        currentMoveDirection = inputCommandSource.CurrentMoveInput;

        if (!base.InitializeStrikerMovement(strikerBoundsCollider))
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
        dashRequested = false;
    }

    private void HandleMoveInputChanged(Vector2 moveDirection)
    {
        currentMoveDirection = moveDirection;

        if (!IsMovementAllowed) return;

        if (moveDirection.sqrMagnitude <= 0.0001f && !dashRequested && !IsDashActive)
            StopMovement();

        UpdateMovementLoopState();
    }

    private void HandleDashInputChanged(bool isDashPressed)
    {
        if (!isDashPressed) return;
        if (!IsMovementAllowed) return;

        dashRequested = true;
        UpdateMovementLoopState();
    }

    private void DisconnectInputEvents()
    {
        if (!inputCommandSource) return;

        inputCommandSource.MoveInputChanged -= HandleMoveInputChanged;
        inputCommandSource.DashInputChanged -= HandleDashInputChanged;
        inputCommandSource.Shutdown();
    }

    private bool CanMoveThisFrame()
    {
        if (IsMovementAllowed) return true;

        StopMovement();
        return false;
    }
}
