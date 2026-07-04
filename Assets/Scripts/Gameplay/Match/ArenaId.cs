using System;

public readonly struct ArenaId : IEquatable<ArenaId>
{
    public ArenaId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Arena id cannot be empty.", nameof(value));

        Value = value;
    }

    public string Value { get; }

    public bool Equals(ArenaId other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is ArenaId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
    }

    public override string ToString()
    {
        return Value ?? string.Empty;
    }

    public static bool operator ==(ArenaId left, ArenaId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArenaId left, ArenaId right)
    {
        return !left.Equals(right);
    }
}
