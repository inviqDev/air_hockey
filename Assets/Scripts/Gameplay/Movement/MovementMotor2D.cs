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
    
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;

    private void Reset()
    {
        commandSourceBehaviour = GetComponent<PlayerInputCommandSource>();
        ConfigureBody(GetComponent<Rigidbody2D>());
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sideOwner = GetComponent<SideOwner>();
        dashAbility = GetComponent<DashAbility>();
        areaLimiter = GetComponent<HalfFieldAreaLimiter>();
        circleCollider = GetComponent<CircleCollider2D>();
        commandSource = GetCommandSource();

        ConfigureBody(rb);
    }

    private void FixedUpdate()
    {
        if (!isMovementAllowed)
        {
            rb.linearVelocity = Vector2.zero;
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
        var predictedPosition = rb.position + velocity * Time.fixedDeltaTime;

        if (areaLimiter.IsPastCenterLine(predictedPosition, sideOwner.Side, worldRadius))
        {
            var clampedPosition = areaLimiter.ClampToCenterLine(rb.position, sideOwner.Side, worldRadius);

            if (clampedPosition != rb.position)
            {
                rb.MovePosition(clampedPosition);
            }

            if (IsMovingAcrossCenterLine(velocity.x, sideOwner.Side))
            {
                velocity.x = 0f;
            }
        }

        rb.linearVelocity = velocity;
    }

    public void SetMovementAllowed(bool isAllowed)
    {
        isMovementAllowed = isAllowed;

        if (!isMovementAllowed && rb)
            rb.linearVelocity = Vector2.zero;
    }

    private void OnDisable()
    {
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
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
        {
            return serializedSource;
        }

        foreach (var behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour is not IMovementCommandSource source) continue;
            commandSourceBehaviour = behaviour;
            return source;
        }

        return null;
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
