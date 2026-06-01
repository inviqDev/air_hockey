public readonly struct MatchConfiguration
{
    public MatchConfiguration(PlayerTwoControlType playerTwoControlType, PlayerSide playerOneSide)
    {
        PlayerTwoControlType = playerTwoControlType;
        PlayerOneSide = playerOneSide;
    }

    public PlayerTwoControlType PlayerTwoControlType { get; }
    public PlayerSide PlayerOneSide { get; }
    public PlayerSide PlayerTwoSide => SideUtility.Opposite(PlayerOneSide);

    public MatchPlayer GetPlayerForSide(PlayerSide side)
    {
        return side == PlayerOneSide ? MatchPlayer.PlayerOne : MatchPlayer.PlayerTwo;
    }
}