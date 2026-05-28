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

    private Rigidbody2D body;
    private SideOwner sideOwner;
    private DashAbility dashAbility;
    private HalfFieldAreaLimiter areaLimiter;
    private CircleCollider2D circleCollider;
    private IMovementCommandSource commandSource;

    private void Reset()
    {
        commandSourceBehaviour = GetComponent<PlayerInputCommandSource>();
        ConfigureBody(GetComponent<Rigidbody2D>());
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        sideOwner = GetComponent<SideOwner>();
        dashAbility = GetComponent<DashAbility>();
        areaLimiter = GetComponent<HalfFieldAreaLimiter>();
        circleCollider = GetComponent<CircleCollider2D>();
        commandSource = commandSourceBehaviour as IMovementCommandSource;

        ConfigureBody(body);
    }

    private void FixedUpdate()
    {
        if (commandSource == null)
        {
            return;
        }

        var command = commandSource.ReadCommand();
        var move = Vector2.ClampMagnitude(command.Move, 1f);
        var dashVelocity = dashAbility.Step(command.DashPressed, sideOwner.Side, Time.fixedDeltaTime);
        var velocity = move * moveSpeed + dashVelocity;
        var worldRadius = GetWorldRadius();
        var predictedPosition = body.position + velocity * Time.fixedDeltaTime;

        if (areaLimiter.IsPastCenterLine(predictedPosition, sideOwner.Side, worldRadius))
        {
            var clampedPosition = areaLimiter.ClampToCenterLine(body.position, sideOwner.Side, worldRadius);

            if (clampedPosition != body.position)
            {
                body.MovePosition(clampedPosition);
            }

            if (IsMovingAcrossCenterLine(velocity.x, sideOwner.Side))
            {
                velocity.x = 0f;
            }
        }

        body.linearVelocity = velocity;
    }

    private void OnDisable()
    {
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
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

    private static void ConfigureBody(Rigidbody2D targetBody)
    {
        targetBody.bodyType = RigidbodyType2D.Dynamic;
        targetBody.gravityScale = 0f;
        targetBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        targetBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        targetBody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}
