public readonly struct StrikerSetupContext
{
    public StrikerSetupContext(PlayerSide side, Puck puck, PlayerControlScheme playerControlScheme)
    {
        Side = side;
        Puck = puck;
        PlayerControlScheme = playerControlScheme;
    }

    public PlayerSide Side { get; }
    public Puck Puck { get; }
    public PlayerControlScheme PlayerControlScheme { get; }
}