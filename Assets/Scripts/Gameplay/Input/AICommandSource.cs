using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class AICommandSource : MonoBehaviour, IMovementCommandSource
{
    [Header("References")]
    [SerializeField] private Rigidbody2D puck;

    [Header("Positions")]
    [SerializeField] private Vector2 defensivePosition = new(-7.5f, 0f);
    [SerializeField] private float centerX = 0f;

    [Header("AI Tuning")]
    [SerializeField, Range(0f, 1f)] private float aggression = 0.95f;
    [SerializeField] private float predictionTime = 0.28f;
    [SerializeField] private float threatTrackSpeedThreshold = 0.08f;
    [SerializeField] private float commitDistance = 3f;

    [Header("Guard Position")]
    [SerializeField] private float guardForwardOffset = 0.85f;
    [SerializeField] private float guardYFollow = 0.45f;
    [SerializeField] private float guardYDeadZone = 0.9f;

    [Header("Strike Logic")]
    [SerializeField] private Vector2 attackDirection = Vector2.right;
    [SerializeField] private float setupDistance = 0.7f;
    [SerializeField] private float behindPuckTolerance = 0.3f;
    [SerializeField] private float strikeDistance = 0.9f;
    [SerializeField] private float sideStepDistance = 0.95f;
    [SerializeField] private float goalCenteringWeight = 0.35f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 1.35f;
    [SerializeField] private float dashCooldown = 0.55f;
    [SerializeField, Range(-1f, 1f)] private float dashDirectionThreshold = 0.25f;

    private PlayerSide side = PlayerSide.Left;
    private float remainingDashCooldown;
    private Rigidbody2D body;
    private Rigidbody2D puckBody;
    private CircleCollider2D bodyCollider;
    private CircleCollider2D puckCollider;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CircleCollider2D>();
        attackDirection = attackDirection.sqrMagnitude > 0.001f ? attackDirection.normalized : Vector2.right;
        puckBody = puck;
        puckCollider = puckBody ? puckBody.GetComponent<CircleCollider2D>() : null;
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public MovementCommand ReadCommand()
    {
        TickCooldown();

        if (!puckBody)
        {
            return MoveToward(defensivePosition, false);
        }

        var puckPosition = puckBody.position;
        var puckVelocity = puckBody.linearVelocity;
        var predictedPuckPosition = PredictPuckPosition(puckPosition, puckVelocity);

        if (!ShouldReactToPuck(puckPosition, puckVelocity))
        {
            return MoveToward(GetGuardTarget(puckPosition), false);
        }

        if (!ShouldCommitToPuck(puckPosition, puckVelocity))
        {
            return MoveToward(GetGuardTarget(predictedPuckPosition), false);
        }

        var aiIsBehindPuck = IsBehindPuck(body.position, puckPosition);

        if (!aiIsBehindPuck)
        {
            var setupTarget = GetSetupTarget(predictedPuckPosition);
            return MoveToward(setupTarget, false);
        }

        if (IsCloseEnoughToStrike(puckPosition))
        {
            var dashRequested = ShouldDash(puckPosition);
            var contactTarget = GetAttackContactTarget(predictedPuckPosition);
            return MoveToward(contactTarget, dashRequested);
        }

        var approachTarget = GetStrikeApproachTarget(predictedPuckPosition);
        return MoveToward(approachTarget, false);
    }

    public void SetPuck(Rigidbody2D puckRigidbody)
    {
        puck = puckRigidbody;
        puckBody = puckRigidbody;
        puckCollider = puckRigidbody ? puckRigidbody.GetComponent<CircleCollider2D>() : null;
    }

    public void SetSide(PlayerSide playerSide)
    {
        side = playerSide;

        var defensiveX = Mathf.Abs(defensivePosition.x);
        defensivePosition.x = side == PlayerSide.Left ? -defensiveX : defensiveX;
        attackDirection = side == PlayerSide.Left ? Vector2.right : Vector2.left;
    }

    private void TickCooldown()
    {
        if (remainingDashCooldown > 0f)
        {
            remainingDashCooldown -= Time.fixedDeltaTime;
        }
    }

    private bool ShouldReactToPuck(Vector2 puckPosition, Vector2 puckVelocity)
    {
        var puckMovingTowardAi = side == PlayerSide.Left
            ? puckVelocity.x < -threatTrackSpeedThreshold
            : puckVelocity.x > threatTrackSpeedThreshold;

        return puckMovingTowardAi || IsPuckOnAiSide(puckPosition);
    }

    private bool ShouldCommitToPuck(Vector2 puckPosition, Vector2 puckVelocity)
    {
        if (IsPuckOnAiSide(puckPosition))
        {
            return true;
        }

        var puckMovingTowardAi = side == PlayerSide.Left
            ? puckVelocity.x < -threatTrackSpeedThreshold
            : puckVelocity.x > threatTrackSpeedThreshold;

        if (!puckMovingTowardAi)
        {
            return false;
        }

        return Mathf.Abs(puckPosition.x - body.position.x) <= GetCommitDistance();
    }

    private Vector2 PredictPuckPosition(Vector2 puckPosition, Vector2 puckVelocity)
    {
        var puckSpeed = puckVelocity.magnitude;
        var dynamicPredictionTime = predictionTime + Mathf.Min(0.14f, puckSpeed * 0.02f) + GetPuckRadius() * 0.06f;
        var predictedPosition = puckPosition + puckVelocity * dynamicPredictionTime;
        predictedPosition.x = ClampToAiSide(predictedPosition.x);

        return predictedPosition;
    }

    private bool IsBehindPuck(Vector2 aiPosition, Vector2 puckPosition)
    {
        var fromPuckToAi = aiPosition - puckPosition;
        var distanceInAttackDirection = Vector2.Dot(fromPuckToAi, attackDirection);

        return distanceInAttackDirection <= GetBehindPuckTolerance();
    }

    private Vector2 GetSetupTarget(Vector2 puckPosition)
    {
        var setupTarget = puckPosition - attackDirection * GetSetupDistance();
        setupTarget.y = Mathf.Lerp(setupTarget.y, 0f, goalCenteringWeight);
        setupTarget.x = ClampToAiSide(setupTarget.x);

        if (!IsPuckBetweenAiAndSetupTarget(puckPosition, setupTarget)) return setupTarget;
        
        var yDirection = body.position.y >= puckPosition.y ? 1f : -1f;
        setupTarget.y += yDirection * GetSideStepDistance();

        return setupTarget;
    }

    private Vector2 GetStrikeApproachTarget(Vector2 puckPosition)
    {
        var approachOffset = Mathf.Min(GetBehindPuckTolerance(), GetSetupDistance() * 0.55f);
        var approachTarget = puckPosition - attackDirection * approachOffset;
        approachTarget.y = Mathf.Lerp(approachTarget.y, 0f, goalCenteringWeight * 0.65f);
        approachTarget.x = ClampToAiSide(approachTarget.x);

        return approachTarget;
    }

    private Vector2 GetAttackContactTarget(Vector2 puckPosition)
    {
        var contactOffset = GetPuckRadius() + GetBodyRadius() * 0.35f;
        var contactTarget = puckPosition - attackDirection * contactOffset;
        contactTarget.y = Mathf.Lerp(contactTarget.y, 0f, goalCenteringWeight);
        contactTarget.x = ClampToAiSide(contactTarget.x);
        return contactTarget;
    }

    private float ClampToAiSide(float x)
    {
        return side == PlayerSide.Left
            ? Mathf.Min(x, centerX - 0.25f)
            : Mathf.Max(x, centerX + 0.25f);
    }

    private bool IsPuckBetweenAiAndSetupTarget(Vector2 puckPosition, Vector2 setupTarget)
    {
        var aiPosition = body.position;

        var puckBetweenX =
            puckPosition.x > Mathf.Min(aiPosition.x, setupTarget.x) &&
            puckPosition.x < Mathf.Max(aiPosition.x, setupTarget.x);

        var closeByY = Mathf.Abs(puckPosition.y - aiPosition.y) < (0.45f + GetPuckRadius());

        return puckBetweenX && closeByY;
    }

    private bool ShouldDash(Vector2 puckPosition)
    {
        if (remainingDashCooldown > 0f)
        {
            return false;
        }

        var toPuck = puckPosition - body.position;
        var sqrDistanceToPuck = toPuck.sqrMagnitude;

        var resolvedDashDistance = GetDashDistance();
        if (sqrDistanceToPuck <= 0.001f || sqrDistanceToPuck > resolvedDashDistance * resolvedDashDistance)
        {
            return false;
        }

        var directionToPuck = toPuck.normalized;
        var puckIsInAttackDirection = Vector2.Dot(directionToPuck, attackDirection) >= dashDirectionThreshold;

        if (!puckIsInAttackDirection)
        {
            return false;
        }

        remainingDashCooldown = dashCooldown;
        return true;
    }

    private bool IsCloseEnoughToStrike(Vector2 puckPosition)
    {
        var toPuck = puckPosition - body.position;
        var resolvedStrikeDistance = GetStrikeDistance();
        return toPuck.sqrMagnitude <= resolvedStrikeDistance * resolvedStrikeDistance;
    }

    private bool IsPuckOnAiSide(Vector2 puckPosition)
    {
        return side == PlayerSide.Left
            ? puckPosition.x < centerX
            : puckPosition.x > centerX;
    }

    private Vector2 GetGuardTarget(Vector2 puckPosition)
    {
        var guardTarget = defensivePosition;
        guardTarget.x += attackDirection.x * guardForwardOffset;

        var yDelta = puckPosition.y - body.position.y;
        if (Mathf.Abs(yDelta) <= guardYDeadZone)
        {
            guardTarget.y = body.position.y;
            return guardTarget;
        }

        guardTarget.y = body.position.y + (yDelta - Mathf.Sign(yDelta) * guardYDeadZone) * guardYFollow;
        return guardTarget;
    }

    private MovementCommand MoveToward(Vector2 target, bool dashRequested)
    {
        var toTarget = target - body.position;

        if (toTarget.sqrMagnitude < 0.01f)
        {
            return new MovementCommand(Vector2.zero, dashRequested);
        }

        var distanceToTarget = toTarget.magnitude;
        var desiredSpeedFactor = distanceToTarget >= 1.25f
            ? 1f
            : Mathf.Lerp(0.35f, 1f, distanceToTarget / 1.25f);
        var move = Vector2.ClampMagnitude(toTarget, 1f) * aggression * desiredSpeedFactor;
        return new MovementCommand(move, dashRequested);
    }

    private float GetCommitDistance()
    {
        return commitDistance + GetPuckRadius() * 2f + GetBodyRadius() * 0.6f;
    }

    private float GetSetupDistance()
    {
        return setupDistance + GetPuckRadius() * 0.8f;
    }

    private float GetBehindPuckTolerance()
    {
        return behindPuckTolerance + GetPuckRadius() * 0.35f;
    }

    private float GetStrikeDistance()
    {
        return strikeDistance + GetPuckRadius() * 0.9f + GetBodyRadius() * 0.2f;
    }

    private float GetSideStepDistance()
    {
        return sideStepDistance + GetPuckRadius() * 0.5f;
    }

    private float GetDashDistance()
    {
        return dashDistance + GetPuckRadius() * 1.1f;
    }

    private float GetBodyRadius()
    {
        if (!bodyCollider)
        {
            return 0.35f;
        }

        var scale = body.transform.lossyScale;
        var largestAxisScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        return bodyCollider.radius * largestAxisScale;
    }

    private float GetPuckRadius()
    {
        if (!puckBody)
        {
            return 0.175f;
        }

        if (!puckCollider)
        {
            puckCollider = puckBody.GetComponent<CircleCollider2D>();
        }

        if (!puckCollider)
        {
            return 0.175f;
        }

        var scale = puckBody.transform.lossyScale;
        var largestAxisScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        return puckCollider.radius * largestAxisScale;
    }

    private void ValidateReferences()
    {
        if (!puck)
        {
            Debug.LogError($"{nameof(AICommandSource)} on {name} requires a puck Rigidbody2D reference.", this);
        }
    }
}