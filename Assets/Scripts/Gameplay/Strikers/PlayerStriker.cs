using UnityEngine;

[RequireComponent(typeof(PlayerInputCommandSource))]
public sealed class PlayerStriker : StrikerBase
{
    [SerializeField] private PlayerInputCommandSource inputCommandSource;

    private void Reset()
    {
        if (!inputCommandSource)
            inputCommandSource = GetComponent<PlayerInputCommandSource>();
    }

    protected override void ApplyStrikerSetup(StrikerSetupContext setupContext)
    {
        if (!inputCommandSource)
            inputCommandSource = GetComponent<PlayerInputCommandSource>();

        inputCommandSource?.SetControlScheme(setupContext.PlayerControlScheme);
    }

    protected override void ResetCustomStrikerState()
    {
        inputCommandSource?.ResetState();
    }
}