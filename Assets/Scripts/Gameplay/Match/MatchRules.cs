public sealed class MatchRules
{
    public MatchRules(int winningScore)
    {
        WinningScore = winningScore;
    }

    public int WinningScore { get; }

    public bool IsWinningScoreReached(int score)
    {
        return WinningScore > 0 && score >= WinningScore;
    }
}
