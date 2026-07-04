public readonly struct ParticipantSlotAssignment
{
    public ParticipantSlotAssignment(ParticipantId participantId, ArenaSlotId slotId)
    {
        ParticipantId = participantId;
        SlotId = slotId;
    }

    public ParticipantId ParticipantId { get; }
    public ArenaSlotId SlotId { get; }
}
