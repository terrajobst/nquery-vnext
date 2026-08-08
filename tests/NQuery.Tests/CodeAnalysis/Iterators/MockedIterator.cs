using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

internal sealed class MockedIterator : Iterator
{
    private readonly IReadOnlyList<object?[]> _rows;
    private readonly MockedRowBuffer _rowBuffer;

    private int _rowIndex;

    private MockedIterator(IReadOnlyList<object?[]> rows, Type[] columnTypes)
    {
        _rows = rows;
        _rowBuffer = new MockedRowBuffer(columnTypes);
    }

    public MockedIterator(object?[] rows)
        : this(rows.Select(v => new[] { v }).ToArray(), RowBufferTestSupport.InferColumnTypes(Validate(rows)))
    {
    }

    public MockedIterator(object?[,] rows)
        : this(ToRowArray(rows), RowBufferTestSupport.InferColumnTypes(rows))
    {
    }

    // Explicit column types -- needed when a column is all-NULL in the data (its type
    // can't be inferred) but must still land in a specific container.
    public MockedIterator(Type[] columnTypes, object?[,] rows)
        : this(ToRowArray(rows), columnTypes)
    {
    }

    public static MockedIterator Empty(params Type[] columnTypes)
    {
        return new MockedIterator(Array.Empty<object?[]>(), columnTypes);
    }

    private static object?[] Validate(object?[] rows)
    {
        if (rows.Any(v => v is object[]))
            throw new ArgumentException("Nested array detected. Use two-dimensional array for multiple columns.");

        return rows;
    }

    private static object?[][] ToRowArray(object?[,] rows)
    {
        var rowCount = rows.GetLength(0);
        var entryCount = rows.GetLength(1);

        var rowArray = new object?[rowCount][];

        for (var i = 0; i < rowCount; i++)
        {
            rowArray[i] = new object?[entryCount];

            for (var j = 0; j < entryCount; j++)
                rowArray[i][j] = rows[i, j];
        }

        return rowArray;
    }

    public override RowBuffer RowBuffer => _rowBuffer;

    public int DisposalCount { get; private set; }

    public int TotalOpenCount { get; private set; }

    public int TotalReadCount { get; private set; }

    public override void Open()
    {
        TotalOpenCount++;
        _rowIndex = 0;
    }

    public override void Dispose()
    {
        DisposalCount++;
    }

    public override bool Read()
    {
        if (_rowIndex == _rows.Count)
            return false;

        _rowBuffer.SetRow(_rows[_rowIndex]);

        TotalReadCount++;
        _rowIndex++;
        return true;
    }
}
