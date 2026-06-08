using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SideOwner))]
[RequireComponent(typeof(HalfFieldAreaLimiter))]
public sealed class StrikerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Dash")]
    [SerializeField, Min(0f)] private float dashSpeed = 16f;
    [SerializeField, Min(0f)] private float dashDuration = 0.12f;
    [SerializeField, Min(0f)] private float dashCooldown = 0.35f;

    private SideOwner sideOwner;
    private HalfFieldAreaLimiter areaLimiter;
    private IMovementCommandSource commandSource;
    
    private Rigidbody2D strikerRb;
    private DashAbility dashAbility;
    
    private bool isMovementAllowed;
    private bool isInitialized;
    
    private float centerLineLimiterOffset;

    private void Reset()
    {
        if (!strikerRb)
            strikerRb = GetComponent<Rigidbody2D>();

        if (strikerRb)
            ConfigureBody(strikerRb);
    }

    public bool Initialize(IMovementCommandSource movementCommandSource, BoxCollider2D strikerBoundsCollider)
    {
        if (movementCommandSource == null)
        {
            Debug.LogError($"{nameof(StrikerMovement)} on {name} requires a movement command source during initialization.", this);
            return false;
        }

        if (!strikerBoundsCollider)
        {
            Debug.LogError($"{nameof(StrikerMovement)} on {name} requires a striker bounds collider during initialization.", this);
            return false;
        }

        centerLineLimiterOffset = CalculateCenterLineLimiterOffset(strikerBoundsCollider);
        
        if (centerLineLimiterOffset <= 0f)
        {
            Debug.LogError($"{nameof(StrikerMovement)} on {name} requires a positive center-line extent during initialization.", this);
            return false;
        }

        commandSource = movementCommandSource;
        
        if (isInitialized) return true;
        if (!CacheReferences()) return false;

        ConfigureBody(strikerRb);
        
        dashAbility = CreateDashAbility();
        
        isInitialized = true;
        return true;
    }

    private void FixedUpdate()
    {
        if (!CanMoveThisStep()) return;

        var command = commandSource.ReadCommand();
        var velocity = CalculateVelocity(command);
        var clampedVelocity = ClampVelocityAtCenterLine(velocity);
        
        ApplyVelocity(clampedVelocity);
    }

    public void SetMovementAllowed(bool isAllowed)
    {
        isMovementAllowed = isAllowed;

        if (!isMovementAllowed && strikerRb)
            StopMovement();
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
    }

    private void OnDisable()
    {
        StopMovement();

        isMovementAllowed = false;
    }

    private void StopMovement()
    {
        if (strikerRb)
            strikerRb.linearVelocity = Vector2.zero;
    }

    private bool CanMoveThisStep()
    {
        if (isMovementAllowed) return true;

        StopMovement();
        return false;
    }

    private Vector2 CalculateVelocity(MovementCommand command)
    {
        var moveVelocity = command.Move * moveSpeed;
        var dashVelocity = dashAbility.Step(command.DashPressed, command.Move, sideOwner.Side, Time.fixedDeltaTime);
        
        return moveVelocity + dashVelocity;
    }

    private Vector2 ClampVelocityAtCenterLine(Vector2 velocity)
    {
        var side = sideOwner.Side;
        var predictedPosition = strikerRb.position + velocity * Time.fixedDeltaTime;
        var allowedCenterLineX = areaLimiter.GetAllowedCenterLineX(side, centerLineLimiterOffset);
        var isPastCenterLine = side == PlayerSide.Left
            ? predictedPosition.x > allowedCenterLineX
            : predictedPosition.x < allowedCenterLineX;

        if (!isPastCenterLine) return velocity;

        ClampPositionToCenterLine(allowedCenterLineX);
        velocity.x = 0f;
        return velocity;
    }

    private void ClampPositionToCenterLine(float allowedCenterLineX)
    {
        var clampedPosition = strikerRb.position;
        clampedPosition.x = allowedCenterLineX;

        if (clampedPosition != strikerRb.position)
            strikerRb.MovePosition(clampedPosition);
    }

    private void ApplyVelocity(Vector2 velocity)
    {
        strikerRb.linearVelocity = velocity;
    }

    private float CalculateCenterLineLimiterOffset(BoxCollider2D strikerBoundsCollider)
    {
        var scaleX = Mathf.Abs(transform.lossyScale.x);
        var scaledHalfWidth = strikerBoundsCollider.size.x * scaleX * 0.5f;
        var scaledOffsetX = Mathf.Abs(strikerBoundsCollider.offset.x * scaleX);

        return scaledOffsetX + scaledHalfWidth;
    }

    private DashAbility CreateDashAbility()
    {
        var dash = new DashAbility(dashSpeed, dashDuration, dashCooldown);
        dash.ResetState();
        
        return dash;
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

        if (!areaLimiter && !TryGetComponent(out areaLimiter))
        {
            Debug.LogError($"{nameof(StrikerMovement)} on {name} requires a {nameof(HalfFieldAreaLimiter)} component.", this);
            hasAllReferences = false;
        }

        return hasAllReferences;
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
