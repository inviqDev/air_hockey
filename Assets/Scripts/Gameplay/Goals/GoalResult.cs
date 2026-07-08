public struct GoalResult
{
    public GoalResult(ParticipantId scoringParticipantId, PlayerSide scoringSide, int leftScore, int rightScore, bool hasWinner)
    {
        ScoringParticipantId = scoringParticipantId;
        ScoringSide = scoringSide;
        LeftScore = leftScore;
        RightScore = rightScore;
        HasWinner = hasWinner;
    }

    public ParticipantId ScoringParticipantId { get; }
    public PlayerSide ScoringSide { get; }
    public int LeftScore { get; }
    public int RightScore { get; }
    public bool HasWinner { get; }
}