using UnityEngine;

[RequireComponent(typeof(PlayerStrikerMovement))]
public sealed class PlayerStriker : StrikerBase
{
    protected override void ApplySetup(StrikerSetupContext setupContext)
    {
    }

    protected override bool TryInitializeMovement()
    {
        if (Movement is PlayerStrikerMovement movement)
        {
            var isInitialized = movement.Initialize();
            return isInitialized;
        }

        Debug.LogError($"{nameof(PlayerStriker)} on {name} requires a {nameof(PlayerStrikerMovement)} component.", this);
        return false;
    }
}
