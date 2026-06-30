using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SideOwner))]
public abstract class StrikerMovement : MonoBehaviour, IMovable, IStrikerMovementOverride
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
    private StrikerDashMotor commandDashMotor;
    private Vector2 movementOverrideDirection;
    private Vector2 movementOverrideVelocity;
    private bool hasMovementOverride;

    protected bool IsInitialized => movableBody != null;
    protected bool IsDashActive => IsAnyDashActive();

    public bool IsMovementAllowed => IsMovementCurrentlyAllowed();
    public Vector2 Position => GetCurrentPosition();
    public Vector2 Velocity => GetCurrentVelocity();
    public Vector2 CurrentMoveDirection => movementOverrideDirection;
    public PlayerSide Side => GetCurrentSide();
    public bool CanUseMovementOverride => CanBeginMovementOverride();

    public void SetMovementAllowed(bool isAllowed)
    {
        if (movableBody == null) return;

        movableBody.SetMovementAllowed(isAllowed);

        if (!isAllowed)
        {
            EndMovementOverride();
            HandleMovementStopped();
        }

        UpdateMovementLoopState();
    }

    public void ResetMovementState(Vector2 position)
    {
        if (movableBody == null) return;

        movableBody.ResetMovementState(position);

        if (commandDashMotor != null)
            commandDashMotor.ResetState();

        EndMovementOverride();
        movementOverrideDirection = Vector2.zero;

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

        movementOverrideDirection = command.Move;
        var velocity = CalculateVelocity(command);
        movableBody.ApplyVelocity(velocity);
    }

    public bool TryBeginMovementOverride()
    {
        if (!CanUseMovementOverride) return false;

        movementOverrideVelocity = Vector2.zero;
        hasMovementOverride = true;
        UpdateMovementLoopState();

        return true;
    }

    public void SetMovementOverrideVelocity(Vector2 velocity)
    {
        if (!hasMovementOverride) return;

        movementOverrideVelocity = velocity;
        UpdateMovementLoopState();
    }

    public void EndMovementOverride()
    {
        if (!hasMovementOverride) return;

        movementOverrideVelocity = Vector2.zero;
        hasMovementOverride = false;
        UpdateMovementLoopState();
    }

    protected void SetCurrentMoveDirection(Vector2 moveDirection)
    {
        movementOverrideDirection = moveDirection;
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
        if (hasMovementOverride)
            return movementOverrideVelocity;

        var moveVelocity = command.Move * moveSpeed;
        var commandDashVelocity = commandDashMotor != null
            ? commandDashMotor.Step(command.DashPressed, command.Move, sideOwner.Side, Time.fixedDeltaTime)
            : Vector2.zero;

        return moveVelocity + commandDashVelocity;
    }

    private void InitializeMovableBody()
    {
        if (movableBody != null) return;
        if (!CacheReferences()) return;

        movableBody = new MovableBody(strikerRb);
        commandDashMotor = CreateDashMotor();
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

    private StrikerDashMotor CreateDashMotor()
    {
        var dash = new StrikerDashMotor(dashSpeed, dashDuration, dashCooldown);
        dash.ResetState();

        return dash;
    }

    private bool IsMovementCurrentlyAllowed()
    {
        return movableBody != null && movableBody.IsMovementAllowed;
    }

    private Vector2 GetCurrentPosition()
    {
        return movableBody != null ? movableBody.Position : Vector2.zero;
    }

    private Vector2 GetCurrentVelocity()
    {
        return movableBody != null ? movableBody.Velocity : Vector2.zero;
    }

    private PlayerSide GetCurrentSide()
    {
        return sideOwner ? sideOwner.Side : PlayerSide.Left;
    }

    private bool CanBeginMovementOverride()
    {
        return IsMovementAllowed && !hasMovementOverride;
    }

    private bool IsAnyDashActive()
    {
        if (commandDashMotor != null && commandDashMotor.IsDashing) return true;

        return hasMovementOverride;
    }
}
