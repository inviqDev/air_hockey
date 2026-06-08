using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SideOwner))]
[RequireComponent(typeof(DashAbility))]
[RequireComponent(typeof(HalfFieldAreaLimiter))]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class MovementMotor2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private MonoBehaviour commandSourceBehaviour;

    private SideOwner sideOwner;
    private DashAbility dashAbility;
    private HalfFieldAreaLimiter areaLimiter;
    private IMovementCommandSource commandSource;
    private bool isMovementAllowed;
    
    private Rigidbody2D strikerRb;
    private CircleCollider2D circleCollider;

    private void Reset()
    {
        commandSourceBehaviour = GetComponent<PlayerInputCommandSource>();
        ConfigureBody(GetComponent<Rigidbody2D>());
    }

    private void Awake()
    {
        if (!CacheReferences()) return;

        ConfigureBody(strikerRb);
        commandSource = GetCommandSource();
    }

    private void FixedUpdate()
    {
        if (!strikerRb) return;

        if (!isMovementAllowed)
        {
            strikerRb.linearVelocity = Vector2.zero;
            return;
        }

        if (commandSource == null)
        {
            return;
        }

        var command = commandSource.ReadCommand();
        var move = Vector2.ClampMagnitude(command.Move, 1f);
        var dashVelocity = dashAbility.Step(command.DashPressed, sideOwner.Side, Time.fixedDeltaTime);
        var velocity = move * moveSpeed + dashVelocity;
        var worldRadius = GetWorldRadius();
        var predictedPosition = strikerRb.position + velocity * Time.fixedDeltaTime;

        if (areaLimiter.IsPastCenterLine(predictedPosition, sideOwner.Side, worldRadius))
        {
            var clampedPosition = areaLimiter.ClampToCenterLine(strikerRb.position, sideOwner.Side, worldRadius);

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
        if (!strikerRb) return;

#if UNITY_6000_0_OR_NEWER
        strikerRb.linearVelocity = Vector2.zero;
#else
        strikerRb.velocity = Vector2.zero;
#endif

        strikerRb.angularVelocity = 0f;
        strikerRb.position = position;
        strikerRb.rotation = 0f;

        if (dashAbility)
            dashAbility.ResetState();
    }

    private void OnDisable()
    {
        if (strikerRb)
        {
            strikerRb.linearVelocity = Vector2.zero;
        }
    }

    private float GetWorldRadius()
    {
        var scale = transform.lossyScale;
        var largestAxisScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)); 
        return circleCollider.radius * largestAxisScale;
    }

    private static bool IsMovingAcrossCenterLine(float xVelocity, PlayerSide side)
    {
        return side == PlayerSide.Left ? xVelocity > 0f : xVelocity < 0f;
    }

    private IMovementCommandSource GetCommandSource()
    {
        if (commandSourceBehaviour is IMovementCommandSource serializedSource)
            return serializedSource;

        foreach (var behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour is not IMovementCommandSource source) continue;
            commandSourceBehaviour = behaviour;
            return source;
        }

        Debug.LogError($"{nameof(MovementMotor2D)} on {name} requires a command source that implements {nameof(IMovementCommandSource)}.", this);
        return null;
    }

    private bool CacheReferences()
    {
        var hasAllReferences = true;

        if (!strikerRb && !TryGetComponent(out strikerRb))
        {
            Debug.LogError($"{nameof(MovementMotor2D)} on {name} requires a {nameof(Rigidbody2D)} component.", this);
            hasAllReferences = false;
        }

        if (!sideOwner && !TryGetComponent(out sideOwner))
        {
            Debug.LogError($"{nameof(MovementMotor2D)} on {name} requires a {nameof(SideOwner)} component.", this);
            hasAllReferences = false;
        }

        if (!dashAbility && !TryGetComponent(out dashAbility))
        {
            Debug.LogError($"{nameof(MovementMotor2D)} on {name} requires a {nameof(DashAbility)} component.", this);
            hasAllReferences = false;
        }

        if (!areaLimiter && !TryGetComponent(out areaLimiter))
        {
            Debug.LogError($"{nameof(MovementMotor2D)} on {name} requires a {nameof(HalfFieldAreaLimiter)} component.", this);
            hasAllReferences = false;
        }

        if (!circleCollider && !TryGetComponent(out circleCollider))
        {
            Debug.LogError($"{nameof(MovementMotor2D)} on {name} requires a {nameof(CircleCollider2D)} component.", this);
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
