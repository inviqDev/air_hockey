using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class AICommandSource : MonoBehaviour, IMovementCommandSource
{
    [Header("References")]
    [SerializeField] private Rigidbody2D puck;

    [Header("Positions")]
    [SerializeField] private Vector2 defensivePosition = new(-7.5f, 0f);
    [SerializeField] private float centerX = 0f;

    [Header("AI Tuning")]
    [SerializeField, Range(0f, 1f)] private float aggression = 0.85f;
    [SerializeField] private float interceptDistance = 5f;
    [SerializeField] private float predictionTime = 0.35f;

    [Header("Strike Logic")]
    [SerializeField] private Vector2 attackDirection = Vector2.right;
    [SerializeField] private float setupDistance = 0.9f;
    [SerializeField] private float strikeDistance = 0.75f;
    [SerializeField] private float sideStepDistance = 0.8f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 1.15f;
    [SerializeField] private float dashCooldown = 0.75f;

    private float remainingDashCooldown;
    private Rigidbody2D body;
    private Rigidbody2D puckBody;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        attackDirection.Normalize();
        
        puckBody ??= GetPuck();
    }

    public MovementCommand ReadCommand()
    {
        TickCooldown();

        if (!puckBody)
        {
            Assert.IsTrue(!puckBody, "No puck found");
            return MoveToward(defensivePosition, false);
        }

        var puckPosition = puckBody.position;
        var puckVelocity = puckBody.linearVelocity;

        if (!ShouldEngagePuck(puckBody))
        {
            return MoveToward(defensivePosition, false);
        }

        var predictedPuckPosition = PredictPuckPosition(puckPosition, puckVelocity);
        var aiIsBehindPuck = IsBehindPuck(body.position, puckPosition);

        if (!aiIsBehindPuck)
        {
            var setupTarget = GetSetupTarget(predictedPuckPosition);
            return MoveToward(setupTarget, false);
        }

        var distanceToPuck = Vector2.Distance(body.position, puckPosition);

        if (distanceToPuck <= strikeDistance)
        {
            var dashRequested = ShouldDash(puckPosition);
            return MoveToward(puckPosition, dashRequested);
        }

        var approachTarget = GetSetupTarget(predictedPuckPosition);
        return MoveToward(approachTarget, false);
    }

    private void TickCooldown()
    {
        if (remainingDashCooldown > 0f)
        {
            remainingDashCooldown -= Time.fixedDeltaTime;
        }
    }

    private Rigidbody2D GetPuck()
    {
        if (puck)
        {
            return puck;
        }

        var puckObject = GameObject.Find("Puck");

        if (puckObject)
        {
            puck = puckObject.GetComponent<Rigidbody2D>();
        }

        return puck;
    }

    private bool ShouldEngagePuck(Rigidbody2D puckRb)
    {
        var puckPosition = puckRb.position;
        var puckVelocity = puckRb.linearVelocity;

        var puckIsOnAiSide = puckPosition.x < centerX;
        var puckMovingTowardAi = puckVelocity.x < -0.05f;
        var puckIsCloseEnough = Mathf.Abs(puckPosition.x - body.position.x) <= interceptDistance;

        return puckIsOnAiSide || puckMovingTowardAi || puckIsCloseEnough;
    }

    private Vector2 PredictPuckPosition(Vector2 puckPosition, Vector2 puckVelocity)
    {
        var predictedPosition = puckPosition + puckVelocity * predictionTime;
        predictedPosition.x = Mathf.Min(predictedPosition.x, centerX - 0.25f);

        return predictedPosition;
    }

    private bool IsBehindPuck(Vector2 aiPosition, Vector2 puckPosition)
    {
        var fromPuckToAi = aiPosition - puckPosition;
        return Vector2.Dot(fromPuckToAi, attackDirection) < 0f;
    }

    private Vector2 GetSetupTarget(Vector2 puckPosition)
    {
        var setupTarget = puckPosition - attackDirection * setupDistance;
        setupTarget.x = Mathf.Min(setupTarget.x, centerX - 0.25f);

        if (!IsPuckBetweenAiAndSetupTarget(puckPosition, setupTarget)) return setupTarget;
        
        var yDirection = body.position.y >= puckPosition.y ? 1f : -1f;
        setupTarget.y += yDirection * sideStepDistance;

        return setupTarget;
    }

    private bool IsPuckBetweenAiAndSetupTarget(Vector2 puckPosition, Vector2 setupTarget)
    {
        var aiPosition = body.position;

        var puckBetweenX =
            puckPosition.x > Mathf.Min(aiPosition.x, setupTarget.x) &&
            puckPosition.x < Mathf.Max(aiPosition.x, setupTarget.x);

        var closeByY = Mathf.Abs(puckPosition.y - aiPosition.y) < 0.6f;

        return puckBetweenX && closeByY;
    }

    private bool ShouldDash(Vector2 puckPosition)
    {
        if (remainingDashCooldown > 0f)
        {
            return false;
        }

        var toPuck = puckPosition - body.position;
        var puckIsInAttackDirection = Vector2.Dot(toPuck.normalized, attackDirection) > 0.4f;

        if (!puckIsInAttackDirection || toPuck.sqrMagnitude > dashDistance * dashDistance)
        {
            return false;
        }

        remainingDashCooldown = dashCooldown;
        return true;
    }

    private MovementCommand MoveToward(Vector2 target, bool dashRequested)
    {
        var toTarget = target - body.position;

        if (toTarget.sqrMagnitude < 0.01f)
        {
            return new MovementCommand(Vector2.zero, dashRequested);
        }

        var move = Vector2.ClampMagnitude(toTarget, 1f) * aggression;
        return new MovementCommand(move, dashRequested);
    }
}