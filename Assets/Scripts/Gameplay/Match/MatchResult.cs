using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public sealed class MatchResult
{
    private readonly ReadOnlyDictionary<ParticipantId, int> finalScoresByParticipant;

    public ParticipantId WinnerParticipantId { get; }
    public IReadOnlyDictionary<ParticipantId, int> FinalScoresByParticipant => finalScoresByParticipant;

    public MatchResult(ParticipantId winnerParticipantId, IReadOnlyDictionary<ParticipantId, int> finalScoresByParticipant)
    {
        if (finalScoresByParticipant == null)
            throw new ArgumentNullException(nameof(finalScoresByParticipant));

        if (finalScoresByParticipant.Count == 0)
            throw new ArgumentException("Match result requires at least one final score.", nameof(finalScoresByParticipant));

        if (!finalScoresByParticipant.ContainsKey(winnerParticipantId))
        {
            throw new ArgumentException(
                $"Match result winner {winnerParticipantId} is missing from final scores.", nameof(winnerParticipantId));
        }

        WinnerParticipantId = winnerParticipantId;
        this.finalScoresByParticipant = new ReadOnlyDictionary<ParticipantId, int>(
            new Dictionary<ParticipantId, int>(finalScoresByParticipant));
    }
}
