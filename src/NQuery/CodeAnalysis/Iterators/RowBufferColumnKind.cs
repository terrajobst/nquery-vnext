namespace NQuery.CodeAnalysis.Iterators;

// A row buffer segregates its columns into four parallel containers by storage shape.
// Reference types (and any value type we don't store unboxed) live in an object[]; the
// remaining value types are stored unboxed, bucketed by width: <= 4 bytes, <= 8 bytes,
// and <= 16 bytes. A column is addressed by (kind, index-within-that-container).
internal enum RowBufferColumnKind
{
    Object,
    Bits32,
    Bits64,
    Bits128
}
