using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SideOwner))]
public abstract class StrikerMovement : MonoBehaviour, IMovable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Dash")]
    [SerializeField, Min(0f)] private float dashSpeed = 16f;
    [SerializeField, Min(0f)] private float dashDuration = 0.12f;
    [SerializeField, Min(0f)] private float dashCooldown = 0.35f;

    private Rigidbody2D strikerRb;
    private SideOwner sideOwner;
    
    private MovableBody movableBody;
    private DashAbility dashAbility;

    protected bool IsInitialized => movableBody != null;
    public bool IsMovementAllowed => movableBody != null && movableBody.IsMovementAllowed;
    public Vector2 Position => movableBody != null ? movableBody.Position : Vector2.zero;
    public Vector2 Velocity => movableBody != null ? movableBody.Velocity : Vector2.zero;

    protected bool IsDashActive => dashAbility != null && dashAbility.IsDashing;

    public void SetMovementAllowed(bool isAllowed)
    {
        if (movableBody == null) return;

        movableBody.SetMovementAllowed(isAllowed);

        if (!isAllowed)
            HandleMovementStopped();

        UpdateMovementLoopState();
    }

    public void ResetMovementState(Vector2 position)
    {
        if (movableBody == null) return;

        movableBody.ResetMovementState(position);

        if (dashAbility != null)
            dashAbility.ResetState();

        HandleMovementReset();
        UpdateMovementLoopState();
    }

    public void StopMovement()
    {
        if (movableBody == null) return;
        movableBody.StopMovement();
    }

    protected bool InitializeStrikerMovement()
    {
        InitializeMovableBody();
        if (movableBody == null) return false;

        UpdateMovementLoopState();
        return true;
    }

    protected void ExecuteMovementStep(MovementCommand command)
    {
        if (movableBody == null) return;

        var velocity = CalculateVelocity(command);
        movableBody.ApplyVelocity(velocity);
    }

    protected abstract void UpdateMovementLoopState();

    protected virtual void HandleMovementStopped()
    {
    }

    protected virtual void HandleMovementReset()
    {
    }

    private void OnDisable()
    {
        StopMovement();
    }

    private Vector2 CalculateVelocity(MovementCommand command)
    {
        var moveVelocity = command.Move * moveSpeed;
        var dashVelocity = dashAbility != null
            ? dashAbility.Step(command.DashPressed, command.Move, sideOwner.Side, Time.fixedDeltaTime)
            : Vector2.zero;

        return moveVelocity + dashVelocity;
    }

    private void InitializeMovableBody()
    {
        if (movableBody != null) return;
        if (!CacheReferences()) return;

        movableBody = new MovableBody(strikerRb);
        dashAbility = CreateDashAbility();
    }

    private bool CacheReferences()
    {
        var hasAllReferences = true;

        if (!strikerRb && !TryGetComponent(out strikerRb))
        {
            Debug.LogError($"{nameof(StrikerMovement)} on {name} requires a {nameof(Rigidbody2D)} component.", this);
            hasAllReferences = false;
        }

        if (!sideOwner && !TryGetComponent(out sideOwner))
        {
            Debug.LogError($"{nameof(StrikerMovement)} on {name} requires a {nameof(SideOwner)} component.", this);
            hasAllReferences = false;
        }

        return hasAllReferences;
    }

    private DashAbility CreateDashAbility()
    {
        var dash = new DashAbility(dashSpeed, dashDuration, dashCooldown);
        dash.ResetState();

        return dash;
    }
}
