using UnityEngine;

public sealed class PlayerStrikerMovement : StrikerMovement
{
    private Vector2 currentMoveDirection;

    public bool Initialize()
    {
        currentMoveDirection = Vector2.zero;
        SetCurrentMoveDirection(currentMoveDirection);

        if (!base.InitializeStrikerMovement())
            return false;

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

    public void SetMoveInput(Vector2 moveDirection)
    {
        currentMoveDirection = moveDirection;
        SetCurrentMoveDirection(currentMoveDirection);

        if (!IsMovementAllowed) return;

        if (moveDirection.sqrMagnitude <= 0.0001f && !IsDashActive)
            StopMovement();

        UpdateMovementLoopState();
    }

    public void ClearMoveInput()
    {
        currentMoveDirection = Vector2.zero;
        SetCurrentMoveDirection(currentMoveDirection);
        StopMovement();
        UpdateMovementLoopState();
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
        ClearMoveInput();
    }

    protected override void HandleMovementReset()
    {
        ClearMoveInput();
    }

    private bool CanMoveThisFrame()
    {
        if (IsMovementAllowed) return true;

        StopMovement();
        return false;
    }
}
