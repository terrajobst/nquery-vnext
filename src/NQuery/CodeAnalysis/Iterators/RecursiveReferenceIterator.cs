namespace NQuery.CodeAnalysis.Iterators;

// The working-table scan inside a recursive member: Open snapshots the recursion
// frontier the driver published through the shared RecursiveWorkTable, and Read
// walks its rows 0..Count -- effectively a table scan over a frozen set. The
// driver re-opens the member subtree each round, so every round re-snapshots.
//
// The exposed buffer must be identity-stable across Opens (consumers resolve their
// slots against it once, at iterator construction), so it is an IndirectedRowBuffer
// sized to the reference's output layout and repointed at the current frontier's
// cursor per Open.
internal sealed class RecursiveReferenceIterator : Iterator
{
    private readonly RecursiveWorkTable _workTable;
    private readonly IndirectedRowBuffer _rowBuffer;

    private SpooledRowStore.Cursor? _cursor;
    private int _count;
    private int _position;

    public RecursiveReferenceIterator(RecursiveWorkTable workTable, int objectCount, int bits32Count, int bits64Count, int bits128Count)
    {
        _workTable = workTable;
        _rowBuffer = new IndirectedRowBuffer(objectCount, bits32Count, bits64Count, bits128Count, new NullRowBuffer(objectCount, bits32Count, bits64Count, bits128Count));
    }

    public override RowBuffer RowBuffer => _rowBuffer;

    public override void Open()
    {
        // The frontier is null only before the first round, and the driver opens the
        // members only after publishing one -- but stay defensive and read it as empty.
        var frontier = _workTable.Frontier;
        _cursor = frontier?.CreateCursor();
        _count = frontier?.Count ?? 0;
        _position = 0;

        if (_cursor is not null)
            _rowBuffer.ActiveRowBuffer = _cursor;
    }

    public override void Dispose()
    {
    }

    public override bool Read()
    {
        if (_position == _count)
            return false;

        _cursor!.Row = _position;
        _position++;
        return true;
    }
}
