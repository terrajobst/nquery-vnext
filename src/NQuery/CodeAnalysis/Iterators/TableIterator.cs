using System.Collections;
using System.Collections.Immutable;

using NQuery.Metadata;

namespace NQuery.CodeAnalysis.Iterators;

internal sealed class TableIterator : Iterator
{
    private readonly TableDefinition _table;
    private readonly ImmutableArray<Action<object, ArrayRowBuffer>> _columnWriters;
    private readonly ArrayRowBuffer _rowBuffer;

    private IEnumerator? _rows;

    public TableIterator(TableDefinition table, RowBufferLayout layout, ImmutableArray<Action<object, ArrayRowBuffer>> columnWriters)
    {
        _table = table;
        _columnWriters = columnWriters;
        _rowBuffer = new ArrayRowBuffer(layout);
    }

    public override RowBuffer RowBuffer
    {
        get { return _rowBuffer; }
    }

    public override void Open()
    {
        if (_rows is not null)
            Dispose();

        _rows = _table.GetRows().GetEnumerator();
    }

    public override void Dispose()
    {
        if (_rows is IDisposable disposable)
            disposable.Dispose();

        _rows = null;
    }

    public override bool Read()
    {
        var rows = _rows!;
        if (!rows.MoveNext())
            return false;

        var current = rows.Current!;
        foreach (var writer in _columnWriters)
            writer(current, _rowBuffer);

        return true;
    }
}
