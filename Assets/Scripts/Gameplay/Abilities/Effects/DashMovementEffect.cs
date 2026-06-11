using UnityEngine;

public sealed class DashMovementEffect : TimedEffect, IMovementVelocityEffect
{
    private readonly Vector2 direction;
    private readonly float speed;

    public DashMovementEffect(Vector2 direction, float speed, float duration)
        : base(duration)
    {
        this.direction = direction;
        this.speed = speed;
    }

    public Vector2 ModifyVelocity(Vector2 currentVelocity, in MovementVelocityEffectContext context)
    {
        return currentVelocity + direction * speed;
    }
}
