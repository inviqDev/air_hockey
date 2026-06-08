using UnityEngine;

public sealed class DashAbility
{
    private readonly float dashSpeed;
    private readonly float dashDuration;
    private readonly float cooldown;

    private float remainingDashTime;
    private float remainingCooldownTime;

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
    }

    public Vector2 Step(bool requested, PlayerSide side, float deltaTime)
    {
        if (remainingCooldownTime > 0f)
        {
            remainingCooldownTime -= deltaTime;
        }

        if (requested && remainingDashTime <= 0f && remainingCooldownTime <= 0f)
        {
            remainingDashTime = dashDuration;
            remainingCooldownTime = cooldown;
        }

        if (remainingDashTime <= 0f)
        {
            return Vector2.zero;
        }

        remainingDashTime -= deltaTime;
        return SideUtility.DashDirection(side) * dashSpeed;
    }
}
