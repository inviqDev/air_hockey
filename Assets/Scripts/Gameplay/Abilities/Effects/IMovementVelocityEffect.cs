using UnityEngine;

public interface IMovementVelocityEffect : IEffect
{
    Vector2 ModifyVelocity(Vector2 currentVelocity, in MovementVelocityEffectContext context);
}
