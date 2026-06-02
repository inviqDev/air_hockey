using UnityEngine;

public sealed class DashAbility : MonoBehaviour
{
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float cooldown = 0.35f;

    private float remainingDashTime;
    private float remainingCooldownTime;

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