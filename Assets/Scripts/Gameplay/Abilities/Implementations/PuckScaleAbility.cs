using UnityEngine;

public sealed class PuckScaleAbility : Ability, IAbilityActivationHandler
{
    private readonly PuckScaleAbilityDefinition definition;
    private float remainingCooldownTime;

    public override bool IsActive => remainingCooldownTime > 0f;

    public PuckScaleAbility(PuckScaleAbilityDefinition definition)
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

        if (!frameContext.OwnerContext.TryFindFirstObject<PuckRegistry>(out var puckRegistry))
            return false;

        var puck = puckRegistry.CurrentPuck;
        if (!puck)
            return false;

        if (!puck.TryGetEffectReceiver(out var effectReceiver))
            return false;

        remainingCooldownTime = definition.Cooldown;
        effectReceiver.ReplaceEffects<PuckScaleEffect>(
            new PuckScaleEffect(definition.ScaleMultiplier, definition.EffectDuration));
        return true;
    }
}
