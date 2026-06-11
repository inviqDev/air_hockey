using UnityEngine;

public sealed class DashAbility : Ability, IAbilityActivationHandler
{
    private readonly DashAbilityDefinition definition;

    private float remainingCooldownTime;

    public override bool IsActive => remainingCooldownTime > 0f;

    public DashAbility(DashAbilityDefinition definition)
    {
        this.definition = definition;
    }

    public override void ResetState()
    {
        remainingCooldownTime = 0f;
    }

    public override void Tick(in AbilityFrameContext frameContext)
    {
        if (remainingCooldownTime > 0f)
            remainingCooldownTime = Mathf.Max(0f, remainingCooldownTime - frameContext.DeltaTime);
    }

    public bool TryActivate(in AbilityFrameContext frameContext)
    {
        if (remainingCooldownTime > 0f)
            return false;

        if (!TryResolveSide(frameContext.OwnerContext, out var side))
            return false;

        if (!TryResolveDashDirection(frameContext.MoveInput, side, out var resolvedDirection))
            return false;

        remainingCooldownTime = definition.Cooldown;
        frameContext.OwnerContext.EffectReceiver.AddEffect(new DashMovementEffect(resolvedDirection, definition.DashSpeed, definition.DashDuration));
        return true;
    }

    private static bool TryResolveSide(AbilityContext context, out PlayerSide side)
    {
        if (context.TryGetOwnerComponent<SideOwner>(out var sideOwner))
        {
            side = sideOwner.Side;
            return true;
        }

        side = PlayerSide.Left;
        return false;
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
