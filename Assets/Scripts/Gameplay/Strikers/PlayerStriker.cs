using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerStrikerMovement))]
public sealed class PlayerStriker : StrikerBase
{
    private PlayerControlScheme controlScheme = PlayerControlScheme.Wasd;

    protected override void ApplySetup(StrikerSetupContext setupContext)
    {
        controlScheme = setupContext.GetRequiredHumanControlScheme();
    }

    protected override bool TryInitializeMovement()
    {
        if (Movement is PlayerStrikerMovement movement)
        {
            var isInitialized = movement.Initialize(controlScheme);
            return isInitialized;
        }

        Debug.LogError($"{nameof(PlayerStriker)} on {name} requires a {nameof(PlayerStrikerMovement)} component.", this);
        return false;
    }
}
