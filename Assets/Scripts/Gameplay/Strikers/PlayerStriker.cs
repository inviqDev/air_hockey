using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerStrikerMovement))]
public sealed class PlayerStriker : StrikerBase
{
    private PlayerControlScheme controlScheme = PlayerControlScheme.Wasd;

    protected override void ApplySetup(StrikerSetupContext setupContext)
    {
        controlScheme = setupContext.PlayerControlScheme;
    }

    protected override bool TryInitializeMovement()
    {
        var playerMovement = Movement as PlayerStrikerMovement;
        if (!playerMovement)
        {
            Debug.LogError($"{nameof(PlayerStriker)} on {name} requires a {nameof(PlayerStrikerMovement)} component.", this);
            return false;
        }

        return playerMovement.Initialize(controlScheme);
    }
}
