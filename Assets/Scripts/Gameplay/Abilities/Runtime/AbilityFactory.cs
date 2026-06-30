using UnityEngine;

public sealed class AbilityFactory
{
    public IAbility CreateAbility(
        AbilityConfig config,
        IStrikerMovementOverride movementOverride,
        IPuckScaleController puckScaleController)
    {
        if (!config)
        {
            Debug.LogError($"{nameof(AbilityFactory)} cannot create an ability because the config is missing.");
            return null;
        }

        switch (config)
        {
            case DashAbilityConfig dashConfig:
                return CreateDashAbility(dashConfig, movementOverride);
            case PuckScaleAbilityConfig puckScaleConfig:
                return CreatePuckScaleAbility(puckScaleConfig, puckScaleController);
        }

        return LogErrorInvalidType(config);
    }

    private static IAbility LogErrorInvalidType(AbilityConfig config)
    {
        var configTypeName = config.GetType().Name;
        Debug.LogError($"{nameof(AbilityFactory)} does not support ability config type {configTypeName}.");
        return null;
    }

    private IAbility CreateDashAbility(DashAbilityConfig config, IStrikerMovementOverride movementOverride)
    {
        if (movementOverride == null)
        {
            Debug.LogError($"{nameof(AbilityFactory)} cannot create {nameof(DashAbility)} because the movement override is missing.");
            return null;
        }

        var context = new DashAbility.Context(movementOverride);
        return new DashAbility(config, context);
    }

    private IAbility CreatePuckScaleAbility(PuckScaleAbilityConfig config, IPuckScaleController puckScaleController)
    {
        if (puckScaleController == null)
        {
            Debug.LogError($"{nameof(AbilityFactory)} cannot create {nameof(PuckScaleAbility)} because the puck scale controller is missing.");
            return null;
        }

        var context = new PuckScaleAbility.Context(puckScaleController);
        return new PuckScaleAbility(config, context);
    }
}
