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
        if (!isInitialized || !strikerRb) return;

        if (!isMovementAllowed)
        {
            strikerRb.linearVelocity = Vector2.zero;
            return;
        }

        if (commandSource == null)
        {
            strikerRb.linearVelocity = Vector2.zero;
            return;
        }

        var command = commandSource.ReadCommand();
        var move = Vector2.ClampMagnitude(command.Move, 1f);
        var dashVelocity = dashAbility.Step(command.DashPressed, sideOwner.Side, Time.fixedDeltaTime);
        var velocity = move * moveSpeed + dashVelocity;
        var predictedPosition = strikerRb.position + velocity * Time.fixedDeltaTime;

        if (areaLimiter.IsPastCenterLine(predictedPosition, sideOwner.Side, centerLineLimiterOffset))
        {
            var clampedPosition = areaLimiter.ClampToCenterLine(strikerRb.position, sideOwner.Side, centerLineLimiterOffset);

            if (clampedPosition != strikerRb.position)
            {
                strikerRb.MovePosition(clampedPosition);
            }

            if (IsMovingAcrossCenterLine(velocity.x, sideOwner.Side))
            {
                velocity.x = 0f;
            }
        }

        strikerRb.linearVelocity = velocity;
    }

    public void SetMovementAllowed(bool isAllowed)
    {
        isMovementAllowed = isAllowed;

        if (!isMovementAllowed && strikerRb)
            strikerRb.linearVelocity = Vector2.zero;
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
        if (strikerRb)
        {
            strikerRb.linearVelocity = Vector2.zero;
        }

        isMovementAllowed = false;
    }

    private static bool IsMovingAcrossCenterLine(float xVelocity, PlayerSide side)
    {
        return side == PlayerSide.Left ? xVelocity > 0f : xVelocity < 0f;
    }

    private float CalculateCenterLineLimiterOffset(BoxCollider2D strikerBoundsCollider)
    {
        var scaleX = Mathf.Abs(transform.lossyScale.x);
        var scaledHalfWidth = strikerBoundsCollider.size.x * scaleX * 0.5f;
        var scaledOffsetX = Mathf.Abs(strikerBoundsCollider.offset.x * scaleX);
        
        var calculatedOffset = scaledOffsetX + scaledHalfWidth;
        print(calculatedOffset);
        
        return calculatedOffset;
    }

    private DashAbility CreateDashAbility()
    {
        return new DashAbility(dashSpeed, dashDuration, dashCooldown);
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
