namespace NQuery.CodeAnalysis.Iterators;

// Re-presents an arbitrary selection of columns (drawn from one or more underlying
// buffers) as a single row in a new column order -- used by projection, concatenation,
// and the outer-reference projection an Apply hands to its right side.
//
// Columns keep their container, so the projection is built per container: the projected
// columns of each kind, in output order, become that container's columns. This matches
// how the consuming operator's slot map assigns container-relative indices.
internal sealed class ProjectedRowBuffer : RowBuffer
{
    private readonly RowBufferEntry[] _objectEntries;
    private readonly RowBufferEntry[] _bits32Entries;
    private readonly RowBufferEntry[] _bits64Entries;
    private readonly RowBufferEntry[] _bits128Entries;

    public ProjectedRowBuffer(IEnumerable<RowBufferEntry> projectedEntries)
    {
        ThrowIfNull(projectedEntries);

        var objects = new List<RowBufferEntry>();
        var bits32 = new List<RowBufferEntry>();
        var bits64 = new List<RowBufferEntry>();
        var bits128 = new List<RowBufferEntry>();

        foreach (var entry in projectedEntries)
        {
            switch (entry.Column.Kind)
            {
                case RowBufferColumnKind.Object:
                    objects.Add(entry);
                    break;
                case RowBufferColumnKind.Bits32:
                    bits32.Add(entry);
                    break;
                case RowBufferColumnKind.Bits64:
                    bits64.Add(entry);
                    break;
                case RowBufferColumnKind.Bits128:
                    bits128.Add(entry);
                    break;
                default:
                    throw ExceptionBuilder.UnexpectedValue(entry.Column.Kind);
            }
        }

        _objectEntries = [.. objects];
        _bits32Entries = [.. bits32];
        _bits64Entries = [.. bits64];
        _bits128Entries = [.. bits128];
    }

    public override int ObjectCount => _objectEntries.Length;

    public override int Bits32Count => _bits32Entries.Length;

    public override int Bits64Count => _bits64Entries.Length;

    public override int Bits128Count => _bits128Entries.Length;

    public override object? GetObject(int index)
    {
        var entry = _objectEntries[index];
        return entry.RowBuffer.GetObject(entry.Column.Index);
    }

    public override uint GetBits32(int index)
    {
        var entry = _bits32Entries[index];
        return entry.RowBuffer.GetBits32(entry.Column.Index);
    }

    public override ulong GetBits64(int index)
    {
        var entry = _bits64Entries[index];
        return entry.RowBuffer.GetBits64(entry.Column.Index);
    }

    public override Int128 GetBits128(int index)
    {
        var entry = _bits128Entries[index];
        return entry.RowBuffer.GetBits128(entry.Column.Index);
    }

    public override bool IsNull32(int index)
    {
        var entry = _bits32Entries[index];
        return entry.RowBuffer.IsNull32(entry.Column.Index);
    }

    public override bool IsNull64(int index)
    {
        var entry = _bits64Entries[index];
        return entry.RowBuffer.IsNull64(entry.Column.Index);
    }

    public override bool IsNull128(int index)
    {
        var entry = _bits128Entries[index];
        return entry.RowBuffer.IsNull128(entry.Column.Index);
    }
}
