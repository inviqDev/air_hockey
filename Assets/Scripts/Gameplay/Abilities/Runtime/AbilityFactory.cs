using UnityEngine;

public sealed class AbilityFactory
{
    public IAbility CreateAbility(AbilityConfig config, IStrikerMovementOverride movementOverride)
    {
        if (!config)
        {
            Debug.LogError($"{nameof(AbilityFactory)} cannot create an ability because the config is missing.");
            return null;
        }

        if (config is DashAbilityConfig dashConfig)
            return CreateDashAbility(dashConfig, movementOverride);

        var configTypeName = config.GetType().Name;
        Debug.LogError($"{nameof(AbilityFactory)} does not support ability config type {configTypeName}.");
        return null;
    }

    private IAbility CreateDashAbility(DashAbilityConfig config, IStrikerMovementOverride movementOverride)
    {
        if (!HasMovementOverride(movementOverride))
        {
            Debug.LogError($"{nameof(AbilityFactory)} cannot create {nameof(DashAbility)} because the movement override is missing.");
            return null;
        }

        var context = new DashAbility.Context(movementOverride);
        return new DashAbility(config, context);
    }

    private static bool HasMovementOverride(IStrikerMovementOverride movementOverride)
    {
        if (movementOverride == null) return false;

        var movementObject = movementOverride as Object;
        if (movementObject == null) return true;

        return movementObject;
    }
}
