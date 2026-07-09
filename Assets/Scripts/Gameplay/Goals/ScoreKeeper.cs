using UnityEngine;

public sealed class ScoreKeeper : MonoBehaviour
{
    [SerializeField] private int winningScore = 7;

    private MatchConfiguration currentConfiguration;
    private MatchRules matchRules;
    private MatchScores matchScores;

    public int LeftScore => GetProjectedScore(PlayerSide.Left);
    public int RightScore => GetProjectedScore(PlayerSide.Right);

    public void ConfigureForMatch(MatchConfiguration configuration)
    {
        currentConfiguration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));

        if (currentConfiguration.ArenaId != TemporaryTwoSideArena.ArenaId)
        {
            throw new System.InvalidOperationException(
                $"{nameof(ScoreKeeper)} only supports arena {TemporaryTwoSideArena.ArenaId}, but received {currentConfiguration.ArenaId}.");
        }

        matchScores = new MatchScores(currentConfiguration.Roster);
        matchRules = new MatchRules(winningScore);
    }

    public GoalResult RegisterGoal(PlayerSide goalSide)
    {
        EnsureConfigured();

        var scoringSlotId = TemporaryTwoSideArena.GetScoringSlotForGoalZoneSide(goalSide);
        var scoringDisplaySide = TemporaryTwoSideArena.GetSideForSlot(scoringSlotId);
        var scoringParticipantId = GetParticipantIdForSlot(scoringSlotId);

        var updatedScore = matchScores.ChangeScore(scoringParticipantId, 1);
        var hasWinner = matchRules.IsWinningScoreReached(updatedScore);

        return new GoalResult(scoringParticipantId, scoringDisplaySide, LeftScore, RightScore, hasWinner);
    }

    public void ResetScores()
    {
        EnsureConfigured();
        matchScores.ResetScores();
    }

    public MatchResult CreateMatchResult(ParticipantId winnerParticipantId)
    {
        EnsureConfigured();
        return new MatchResult(winnerParticipantId, matchScores.CreateSnapshot());
    }

    private int GetProjectedScore(PlayerSide scoreDisplaySide)
    {
        if (matchScores == null || currentConfiguration == null)
            return 0;

        var participantId = GetParticipantIdForDisplaySide(scoreDisplaySide);
        return matchScores.GetScore(participantId);
    }

    private ParticipantId GetParticipantIdForDisplaySide(PlayerSide scoreDisplaySide)
    {
        EnsureConfigured();

        var slotId = TemporaryTwoSideArena.GetSlotForSide(scoreDisplaySide);
        return GetParticipantIdForSlot(slotId);
    }

    private ParticipantId GetParticipantIdForSlot(ArenaSlotId slotId)
    {
        EnsureConfigured();

        return currentConfiguration.SlotAssignments.GetParticipantForSlot(slotId);
    }

    private void EnsureConfigured()
    {
        if (currentConfiguration == null)
            throw new System.InvalidOperationException($"{nameof(ScoreKeeper)} requires an active match configuration.");

        if (matchScores == null)
            throw new System.InvalidOperationException($"{nameof(ScoreKeeper)} requires an active score state.");

        if (matchRules == null)
            throw new System.InvalidOperationException($"{nameof(ScoreKeeper)} requires active match rules.");
    }
}
