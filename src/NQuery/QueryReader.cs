using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Emit;
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
    private Delegate?[]? _fieldReaders;

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

    // Reads a column as T without boxing it through object. The typed reader is compiled once
    // per column and reused across rows; reading the same column as a different T recompiles it.
    // SQL NULL yields null for a nullable T and throws for a non-nullable value type, so read a
    // nullable T when the column can be NULL.
    public T GetFieldValue<T>(int columnIndex)
    {
        if (_isBof || _iterator is null)
            throw new InvalidOperationException(Resources.InvalidAttemptToRead);

        _fieldReaders ??= new Delegate?[_entries.Length];
        if (_fieldReaders[columnIndex] is not Func<RowBuffer, T> reader)
        {
            var entry = _entries[columnIndex];
            reader = ColumnReaderCompiler.CompileOrThrow<T>(entry.Column, entry.Type, columnIndex);
            _fieldReaders[columnIndex] = reader;
        }

        return reader(_entries[columnIndex].RowBuffer);
    }

    // Applies a reader compiled elsewhere (CompiledExpression<T>) against the current row's
    // buffer -- the entry resolves to the same column the reader was compiled for.
    internal T ReadField<T>(int columnIndex, Func<RowBuffer, T> reader)
    {
        if (_isBof || _iterator is null)
            throw new InvalidOperationException(Resources.InvalidAttemptToRead);

        return reader(_entries[columnIndex].RowBuffer);
    }

    public int ColumnCount
    {
        get { return _entries.Length; }
    }
}
