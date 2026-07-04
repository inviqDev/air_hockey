using System;

public readonly struct ArenaSlotId : IEquatable<ArenaSlotId>
{
    public ArenaSlotId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Arena slot id cannot be empty.", nameof(value));

        Value = value;
    }

    public string Value { get; }

    public bool Equals(ArenaSlotId other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is ArenaSlotId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
    }

    public override string ToString()
    {
        return Value ?? string.Empty;
    }

    public static bool operator ==(ArenaSlotId left, ArenaSlotId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArenaSlotId left, ArenaSlotId right)
    {
        return !left.Equals(right);
    }
}
