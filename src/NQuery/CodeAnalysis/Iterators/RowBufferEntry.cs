namespace NQuery.CodeAnalysis.Iterators;

// A resolved reference to one column of one row buffer: the buffer, the column's
// container address, and the column's (non-nullable) CLR type. GetValue() boxes the
// value for the object-typed boundaries (sort/top comparisons, the output reader);
// the typed, allocation-free path runs through the compiled expressions instead.
internal readonly struct RowBufferEntry : IEquatable<RowBufferEntry>
{
    public RowBufferEntry(RowBuffer rowBuffer, RowBufferColumn column, Type type)
    {
        RowBuffer = rowBuffer;
        Column = column;
        Type = type;
    }

    public RowBuffer RowBuffer { get; }

    public RowBufferColumn Column { get; }

    public Type Type { get; }

    public object? GetValue()
    {
        return RowBuffer.GetBoxedValue(Column, Type);
    }

    public bool Equals(RowBufferEntry other)
    {
        return RowBuffer == other.RowBuffer &&
               Column.Equals(other.Column);
    }

    public override bool Equals(object? obj)
    {
        return obj is RowBufferEntry other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RowBuffer, Column);
    }

    public static bool operator ==(RowBufferEntry left, RowBufferEntry right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RowBufferEntry left, RowBufferEntry right)
    {
        return !left.Equals(right);
    }
}
