public static class TemporaryTwoSideArena
{
    public static readonly ArenaId ArenaId = new("temporary_two_side");
    public static readonly ArenaSlotId LeftSlot = new("left");
    public static readonly ArenaSlotId RightSlot = new("right");

    public static ArenaSlotId GetSlotForSide(PlayerSide side)
    {
        return side switch
        {
            PlayerSide.Left => LeftSlot,
            PlayerSide.Right => RightSlot,
            _ => throw new System.ArgumentOutOfRangeException(nameof(side), side, "Unsupported temporary arena side.")
        };
    }

    public static PlayerSide GetSideForSlot(ArenaSlotId slotId)
    {
        if (slotId == LeftSlot)
            return PlayerSide.Left;

        if (slotId == RightSlot)
            return PlayerSide.Right;

        throw new System.ArgumentOutOfRangeException(
            nameof(slotId), slotId, $"{nameof(TemporaryTwoSideArena)} only supports left and right slots.");
    }

    public static ArenaSlotId GetScoringSlotForGoalZoneSide(PlayerSide goalSide)
    {
        return goalSide switch
        {
            PlayerSide.Left => RightSlot,
            PlayerSide.Right => LeftSlot,
            _ => throw new System.ArgumentOutOfRangeException(
                nameof(goalSide), goalSide, "Unsupported temporary arena goal zone side.")
        };
    }
}
