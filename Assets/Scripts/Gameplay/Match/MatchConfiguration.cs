using System;

public sealed class MatchConfiguration
{
    public ArenaId ArenaId { get; }
    public ParticipantRoster Roster { get; }
    public ParticipantSlotAssignments SlotAssignments { get; }
    public int ParticipantCount => Roster.Count;

    public MatchConfiguration(ArenaId arenaId, ParticipantRoster roster, ParticipantSlotAssignments slotAssignments)
    {
        if (string.IsNullOrWhiteSpace(arenaId.Value))
            throw new ArgumentException("Arena id cannot be empty.", nameof(arenaId));

        ArenaId = arenaId;
        Roster = roster ?? throw new ArgumentNullException(nameof(roster));
        SlotAssignments = slotAssignments ?? throw new ArgumentNullException(nameof(slotAssignments));

        ValidateAssignmentCoverage(Roster, SlotAssignments);
    }

    private static void ValidateAssignmentCoverage(ParticipantRoster roster, ParticipantSlotAssignments slotAssignments)
    {
        if (roster.Count != slotAssignments.Count)
            throw new ArgumentException($"{nameof(MatchConfiguration)} requires one slot assignment per participant.", nameof(slotAssignments));

        foreach (var participant in roster.Participants)
        {
            if (slotAssignments.ContainsParticipant(participant.ParticipantId)) continue;

            throw new ArgumentException(
                $"{nameof(MatchConfiguration)} is missing slot assignment for participant {participant.ParticipantId}.", nameof(slotAssignments));
        }
    }
}
