using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;

namespace NQuery;

public sealed class QueryReader : IDisposable
{
    private readonly ImmutableArray<string> _columnNames;
    private readonly ImmutableArray<Type> _columnTypes;
    private readonly bool _schemaOnly;
    private readonly RowBufferEntry[] _entries;

    private Iterator? _iterator;
    private bool _isBof;

    internal QueryReader(Iterator iterator, ImmutableArray<(string ColumnName, Type ColumnType)> columnNamesAndTypes, ImmutableArray<ValueSlot> outputValueSlots, bool schemaOnly)
    {
        _iterator = iterator;
        _schemaOnly = schemaOnly;
        _columnNames = columnNamesAndTypes.Select(t => t.ColumnName).ToImmutableArray();
        _columnTypes = columnNamesAndTypes.Select(t => t.ColumnType).ToImmutableArray();

        // Resolve each output column to its row-buffer address once; reading a column
        // then boxes that one cell on demand.
        var allocation = new RowBufferAllocation(null, iterator.RowBuffer, outputValueSlots);
        _entries = outputValueSlots.Select(s => allocation[s]).ToArray();

        if (!_schemaOnly)
            _iterator.Open();

        _isBof = true;
    }

    public void Dispose()
    {
        if (_iterator is null)
            return;

        _iterator.Dispose();
        _iterator = null;
    }

    public bool Read()
    {
        if (_schemaOnly)
            return false;

        if (_iterator!.Read())
        {
            _isBof = false;
            return true;
        }

        return false;
    }

    public string GetColumnName(int columnIndex)
    {
        return _columnNames[columnIndex];
    }

    public Type GetColumnType(int columnIndex)
    {
        return _columnTypes[columnIndex];
    }

    public object this[int columnIndex]
    {
        get
        {
            if (_isBof || _iterator is null)
                throw new InvalidOperationException(Resources.InvalidAttemptToRead);

            return _entries[columnIndex].GetValue()!;
        }
    }

    public int ColumnCount
    {
        get { return _entries.Length; }
    }
}
