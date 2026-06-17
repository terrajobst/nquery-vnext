namespace NQuery.CodeAnalysis.Iterators;

// Addresses a single column inside a row buffer: which container holds it, and its
// index within that container. Slots are no longer absolute positions in a flat row;
// they are container-relative, so a reader knows which Read*/Write* method to call.
internal readonly struct RowBufferColumn : IEquatable<RowBufferColumn>
{
    public RowBufferColumn(RowBufferColumnKind kind, int index)
    {
        Kind = kind;
        Index = index;
    }

    public RowBufferColumnKind Kind { get; }

    public int Index { get; }

    // Picks the container for a slot's (non-nullable) CLR type. Reference types -- and
    // any value type we don't decode unboxed (enums, exotic structs, > 16 bytes) -- go
    // to the object container; the listed value types are bucketed by width.
    public static RowBufferColumnKind Classify(Type type)
    {
        if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte) ||
            type == typeof(short) || type == typeof(ushort) || type == typeof(char) ||
            type == typeof(int) || type == typeof(uint) || type == typeof(float))
            return RowBufferColumnKind.Bits32;

        if (type == typeof(long) || type == typeof(ulong) || type == typeof(double) ||
            type == typeof(DateTime) || type == typeof(TimeSpan))
            return RowBufferColumnKind.Bits64;

        if (type == typeof(decimal) || type == typeof(Guid))
            return RowBufferColumnKind.Bits128;

        return RowBufferColumnKind.Object;
    }

    public bool Equals(RowBufferColumn other)
    {
        return Kind == other.Kind && Index == other.Index;
    }

    public override bool Equals(object? obj)
    {
        return obj is RowBufferColumn other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Kind, Index);
    }

    public override string ToString()
    {
        return $"{Kind}[{Index}]";
    }
}
