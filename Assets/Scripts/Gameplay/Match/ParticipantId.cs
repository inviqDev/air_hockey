using System;

public readonly struct ParticipantId : IEquatable<ParticipantId>
{
    public ParticipantId(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Participant id must be non-negative.");

        Value = value;
    }

    public int Value { get; }

    public bool Equals(ParticipantId other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is ParticipantId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static bool operator ==(ParticipantId left, ParticipantId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ParticipantId left, ParticipantId right)
    {
        return !left.Equals(right);
    }
}
