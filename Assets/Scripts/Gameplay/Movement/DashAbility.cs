using UnityEngine;

public sealed class DashAbility
{
    private readonly float dashSpeed;
    private readonly float dashDuration;
    private readonly float cooldown;

    private float remainingDashTime;
    private float remainingCooldownTime;
    private Vector2 activeDashDirection;

    public bool IsDashing => remainingDashTime > 0f;

    public DashAbility(float dashSpeed, float dashDuration, float cooldown)
    {
        this.dashSpeed = dashSpeed;
        this.dashDuration = dashDuration;
        this.cooldown = cooldown;
    }

    public void ResetState()
    {
        remainingDashTime = 0f;
        remainingCooldownTime = 0f;
        activeDashDirection = Vector2.zero;
    }

    public Vector2 Step(bool requested, Vector2 moveDirection, PlayerSide side, float deltaTime)
    {
        if (remainingCooldownTime > 0f)
        {
            remainingCooldownTime -= deltaTime;
        }

        if (requested && remainingDashTime <= 0f && remainingCooldownTime <= 0f)
        {
            if (!TryResolveDashDirection(moveDirection, side, out var resolvedDirection))
                return Vector2.zero;

            activeDashDirection = resolvedDirection;
            remainingDashTime = dashDuration;
            remainingCooldownTime = cooldown;
        }

        if (remainingDashTime <= 0f)
        {
            return Vector2.zero;
        }

        remainingDashTime -= deltaTime;
        return activeDashDirection * dashSpeed;
    }

    private static bool TryResolveDashDirection(Vector2 moveDirection, PlayerSide side, out Vector2 resolvedDirection)
    {
        var hasInvalidXDirection = side == PlayerSide.Left
            ? moveDirection.x < 0f
            : moveDirection.x > 0f;

        if (hasInvalidXDirection)
        {
            resolvedDirection = Vector2.zero;
            return false;
        }

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            resolvedDirection = moveDirection.normalized;
            return true;
        }

        resolvedDirection = Vector2.zero;
        return false;
    }
}
