using System;

public readonly struct StrikerSetupContext
{
    private readonly InputLayout inputLayout;
    private readonly bool hasHumanInputLayout;

    public PlayerSide Side { get; }
    public Puck Puck { get; }

    public StrikerSetupContext(PlayerSide side, Puck puck, InputLayout inputLayout)
    {
        Side = side;
        Puck = puck;
        this.inputLayout = inputLayout;
        hasHumanInputLayout = true;
    }

    public StrikerSetupContext(PlayerSide side, Puck puck)
    {
        Side = side;
        Puck = puck;
        inputLayout = default;
        hasHumanInputLayout = false;
    }

    public InputLayout GetRequiredHumanInputLayout()
    {
        if (!hasHumanInputLayout)
            throw new InvalidOperationException($"{nameof(StrikerSetupContext)} does not contain a human input layout.");

        return inputLayout;
    }
}
