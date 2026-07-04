using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public sealed class ParticipantRoster
{
    private readonly ReadOnlyCollection<MatchParticipantSetup> participants;
    private readonly Dictionary<ParticipantId, MatchParticipantSetup> participantsById = new();

    public IReadOnlyList<MatchParticipantSetup> Participants => participants;
    public int Count => participants.Count;

    public ParticipantRoster(params MatchParticipantSetup[] participantSetups)
    {
        if (participantSetups == null)
            throw new ArgumentNullException(nameof(participantSetups));

        if (participantSetups.Length == 0)
            throw new ArgumentException("At least one participant setup is required.", nameof(participantSetups));

        var snapshot = participantSetups.ToArray();
        foreach (var setup in snapshot)
        {
            if (setup == null)
                throw new ArgumentException("Participant setup cannot be null.", nameof(participantSetups));

            AddParticipant(setup);
        }

        participants = Array.AsReadOnly(snapshot);
    }

    public bool ContainsParticipant(ParticipantId participantId) => participantsById.ContainsKey(participantId);

    private void AddParticipant(MatchParticipantSetup participantSetup)
    {
        if (!participantsById.TryAdd(participantSetup.ParticipantId, participantSetup))
            throw new InvalidOperationException($"Duplicate participant id {participantSetup.ParticipantId}.");
    }

    public MatchParticipantSetup GetParticipantSetupById(ParticipantId participantId)
    {
        if (participantsById.TryGetValue(participantId, out var setup))
        {
            return setup;
        }

        throw new ArgumentOutOfRangeException(
            nameof(participantId), participantId, $"{nameof(ParticipantRoster)} has no participant setup with id {participantId}.");
    }
}
