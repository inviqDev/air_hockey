public readonly struct StrikerSetupContext
{
    public PlayerSide Side { get; }
    public Puck Puck { get; }

    public StrikerSetupContext(PlayerSide side, Puck puck)
    {
        Side = side;
        Puck = puck;
    }
}
