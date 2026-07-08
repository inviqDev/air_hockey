using System;
using System.Collections.Generic;

public sealed class MatchScores
{
    private readonly Dictionary<ParticipantId, int> scoresByParticipant = new();

    public MatchScores(ParticipantRoster roster)
    {
        if (roster == null)
            throw new ArgumentNullException(nameof(roster));

        foreach (var participant in roster.Participants)
        {
            scoresByParticipant.Add(participant.ParticipantId, 0);
        }
    }

    public int ChangeScore(ParticipantId participantId, int amount = 1)
    {
        EnsureParticipantExists(participantId);
        var updatedScore = scoresByParticipant[participantId] + amount;
        scoresByParticipant[participantId] = Math.Max(0, updatedScore);
        return scoresByParticipant[participantId];
    }

    public int GetScore(ParticipantId participantId)
    {
        EnsureParticipantExists(participantId);
        return scoresByParticipant[participantId];
    }

    public void ResetScores()
    {
        var participantIds = new ParticipantId[scoresByParticipant.Count];
        scoresByParticipant.Keys.CopyTo(participantIds, 0);

        foreach (var participantId in participantIds)
        {
            scoresByParticipant[participantId] = 0;
        }
    }

    private void EnsureParticipantExists(ParticipantId participantId)
    {
        if (scoresByParticipant.ContainsKey(participantId)) return;

        throw new ArgumentOutOfRangeException(
            nameof(participantId), participantId, $"{nameof(MatchScores)} has no score entry for participant {participantId}.");
    }
}
