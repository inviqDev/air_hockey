using System;

public readonly struct StrikerSetupContext
{
    private readonly PlayerControlScheme humanControlScheme;
    private readonly bool hasHumanControlScheme;

    public PlayerSide Side { get; }
    public Puck Puck { get; }

    public StrikerSetupContext(PlayerSide side, Puck puck, PlayerControlScheme playerControlScheme)
    {
        Side = side;
        Puck = puck;
        humanControlScheme = playerControlScheme;
        hasHumanControlScheme = true;
    }

    public StrikerSetupContext(PlayerSide side, Puck puck)
    {
        Side = side;
        Puck = puck;
        humanControlScheme = default;
        hasHumanControlScheme = false;
    }

    public PlayerControlScheme GetRequiredHumanControlScheme()
    {
        if (!hasHumanControlScheme)
            throw new InvalidOperationException($"{nameof(StrikerSetupContext)} does not contain a human control scheme.");

        return humanControlScheme;
    }
}
