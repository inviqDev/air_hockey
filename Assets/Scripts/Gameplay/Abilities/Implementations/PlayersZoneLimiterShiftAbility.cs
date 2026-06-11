using UnityEngine;

public sealed class PlayersZoneLimiterShiftAbility : Ability, IAbilityActivationHandler
{
    private readonly PlayersZoneLimiterShiftAbilityDefinition definition;
    private PlayersZoneLimiter activeLimiter;
    private float remainingEffectTime;
    private float remainingCooldownTime;

    public override bool IsActive => remainingEffectTime > 0f || remainingCooldownTime > 0f;

    public PlayersZoneLimiterShiftAbility(PlayersZoneLimiterShiftAbilityDefinition definition)
    {
        this.definition = definition;
    }

    public override void Tick(in AbilityFrameContext frameContext)
    {
        if (remainingCooldownTime > 0f)
            remainingCooldownTime = Mathf.Max(0f, remainingCooldownTime - frameContext.DeltaTime);

        if (remainingEffectTime <= 0f)
            return;

        remainingEffectTime = Mathf.Max(0f, remainingEffectTime - frameContext.DeltaTime);
        if (remainingEffectTime <= 0f)
            EndActiveShift();
    }

    public override void ResetState()
    {
        remainingCooldownTime = 0f;
        EndActiveShift();
    }

    public override void Dispose()
    {
        EndActiveShift();
    }

    public bool TryActivate(in AbilityFrameContext frameContext)
    {
        if (remainingCooldownTime > 0f || remainingEffectTime > 0f)
            return false;

        if (!frameContext.OwnerContext.TryGetOwnerComponent<SideOwner>(out var sideOwner))
            return false;

        if (!TryResolvePlayersZoneLimiter(frameContext.OwnerContext, out var playersZoneLimiter))
            return false;

        var targetGoalSide = sideOwner.Side == PlayerSide.Left
            ? PlayerSide.Right
            : PlayerSide.Left;

        playersZoneLimiter.ShiftTowardGoal(targetGoalSide, definition.ShiftDistance);
        activeLimiter = playersZoneLimiter;
        remainingEffectTime = definition.EffectDuration;
        remainingCooldownTime = definition.Cooldown;
        return true;
    }

    private void EndActiveShift()
    {
        remainingEffectTime = 0f;

        if (activeLimiter)
            activeLimiter.ResetState();

        activeLimiter = null;
    }

    private static bool TryResolvePlayersZoneLimiter(AbilityContext context, out PlayersZoneLimiter playersZoneLimiter)
    {
        return context.TryFindFirstObjectIncludingInactive(out playersZoneLimiter) && playersZoneLimiter;
    }
}
