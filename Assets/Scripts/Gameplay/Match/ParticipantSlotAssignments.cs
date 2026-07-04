using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public sealed class ParticipantSlotAssignments
{
    private readonly ReadOnlyCollection<ParticipantSlotAssignment> placements;
    private readonly Dictionary<ParticipantId, ParticipantSlotAssignment> byParticipantId = new();
    private readonly Dictionary<ArenaSlotId, ParticipantSlotAssignment> bySlotId = new();

    public IReadOnlyList<ParticipantSlotAssignment> Placements => placements;
    public int Count => placements.Count;

    public ParticipantSlotAssignments(params ParticipantSlotAssignment[] placements)
    {
        if (placements == null)
            throw new ArgumentNullException(nameof(placements));

        if (placements.Length == 0)
            throw new ArgumentException("At least one participant placement is required.", nameof(placements));

        var snapshot = placements.ToArray();
        foreach (var placement in snapshot)
        {
            AddPlacement(placement);
        }

        this.placements = Array.AsReadOnly(snapshot);
    }

    private void AddPlacement(ParticipantSlotAssignment slotAssignment)
    {
        if (string.IsNullOrWhiteSpace(slotAssignment.SlotId.Value))
            throw new InvalidOperationException("Participant slotAssignment requires a non-empty arena slot id.");

        if (byParticipantId.ContainsKey(slotAssignment.ParticipantId))
            throw new InvalidOperationException($"Duplicate participant slotAssignment for {slotAssignment.ParticipantId}.");

        if (bySlotId.ContainsKey(slotAssignment.SlotId))
            throw new InvalidOperationException($"Duplicate arena slot slotAssignment for {slotAssignment.SlotId}.");

        byParticipantId.Add(slotAssignment.ParticipantId, slotAssignment);
        bySlotId.Add(slotAssignment.SlotId, slotAssignment);
    }

    public bool ContainsParticipant(ParticipantId participantId) => byParticipantId.ContainsKey(participantId);

    public ArenaSlotId GetSlotForParticipant(ParticipantId participantId)
    {
        if (byParticipantId.TryGetValue(participantId, out var placement))
        {
            return placement.SlotId;
        }

        throw new ArgumentOutOfRangeException(
            nameof(participantId), participantId, $"{nameof(ParticipantSlotAssignments)} has no placement for participant {participantId}.");
    }

    public ParticipantId GetParticipantForSlot(ArenaSlotId slotId)
    {
        if (bySlotId.TryGetValue(slotId, out var placement))
        {
            return placement.ParticipantId;
        }

        throw new ArgumentOutOfRangeException(
            nameof(slotId), slotId, $"{nameof(ParticipantSlotAssignments)} has no participant assigned to slot {slotId}.");
    }
}