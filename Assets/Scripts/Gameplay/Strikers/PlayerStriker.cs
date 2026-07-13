using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerStrikerMovement))]
public sealed class PlayerStriker : StrikerBase
{
    private InputLayout inputLayout = InputLayout.Wasd;

    protected override void ApplySetup(StrikerSetupContext setupContext)
    {
        inputLayout = setupContext.GetRequiredHumanInputLayout();
    }

    protected override bool TryInitializeMovement()
    {
        if (Movement is PlayerStrikerMovement movement)
        {
            var isInitialized = movement.Initialize(inputLayout);
            return isInitialized;
        }

        Debug.LogError($"{nameof(PlayerStriker)} on {name} requires a {nameof(PlayerStrikerMovement)} component.", this);
        return false;
    }
}
