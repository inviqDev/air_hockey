using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SideOwner))]
public abstract class StrikerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Dash")]
    [SerializeField, Min(0f)] private float dashSpeed = 16f;
    [SerializeField, Min(0f)] private float dashDuration = 0.12f;
    [SerializeField, Min(0f)] private float dashCooldown = 0.35f;

    private SideOwner sideOwner;
    private DashAbility dashAbility;
    
    private Rigidbody2D strikerRb;

    private bool isInitialized;
    private bool isMovementAllowed;

    protected bool IsMovementAllowed => isMovementAllowed;
    protected bool IsInitialized => isInitialized;
    protected bool IsDashActive => dashAbility != null && dashAbility.IsDashing;

    private void Reset()
    {
        if (!strikerRb)
            strikerRb = GetComponent<Rigidbody2D>();

        if (strikerRb)
            ConfigureBody(strikerRb);
    }

    protected bool InitializeStrikerMovement()
    {
        if (!EnsureInitialized()) return false;

        UpdateMovementLoopState();
        return true;
    }

    public void SetMovementAllowed(bool isAllowed)
    {
        isMovementAllowed = isAllowed;

        if (!isMovementAllowed)
        {
            HandleMovementStopped();
            StopMovement();
        }

        UpdateMovementLoopState();
    }

    public void ResetMovementState(Vector2 position)
    {
        if (!isInitialized) return;
        if (!strikerRb) return;

#if UNITY_6000_0_OR_NEWER
        strikerRb.linearVelocity = Vector2.zero;
#else
        strikerRb.velocity = Vector2.zero;
#endif

        strikerRb.angularVelocity = 0f;
        strikerRb.position = position;
        strikerRb.rotation = 0f;

        if (dashAbility != null)
            dashAbility.ResetState();

        HandleMovementReset();
        UpdateMovementLoopState();
    }

    protected void ExecuteMovementStep(MovementCommand command)
    {
        var velocity = CalculateVelocity(command);
        ApplyVelocity(velocity);
    }

    protected void StopMovement()
    {
        if (strikerRb)
            strikerRb.linearVelocity = Vector2.zero;
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
        var dashVelocity = dashAbility.Step(command.DashPressed, command.Move, sideOwner.Side, Time.fixedDeltaTime);

        return moveVelocity + dashVelocity;
    }

    private void ApplyVelocity(Vector2 velocity)
    {
        strikerRb.linearVelocity = velocity;
    }

    private bool EnsureInitialized()
    {
        if (isInitialized)
            return true;

        if (!CacheReferences()) return false;

        ConfigureBody(strikerRb);
        dashAbility = CreateDashAbility();

        isInitialized = true;
        return true;
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

    private static void ConfigureBody(Rigidbody2D targetBody)
    {
        targetBody.bodyType = RigidbodyType2D.Dynamic;
        targetBody.gravityScale = 0f;
        targetBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        targetBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        targetBody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
