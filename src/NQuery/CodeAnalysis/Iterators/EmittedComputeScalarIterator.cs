namespace NQuery.CodeAnalysis.Iterators;

// Computes its columns by running the compiled writer over the input row, storing the
// results typed into its own appended buffer. The writer takes the row buffer as a
// parameter, so it is compiled once and shared across executions.
//
// When this compute is the correlated part of an Apply's right side, its writers
// reference the outer (left) row. The outer buffer is then prepended to the input
// buffer (matching the (outer ++ input) slot layout the writers were compiled
// against); the computed columns are still appended to the input's own columns.
internal sealed class EmittedComputeScalarIterator : Iterator
{
    private readonly Iterator _input;
    private readonly Action<RowBuffer, ArrayRowBuffer> _writer;
    private readonly RowBuffer _functionRowBuffer;
    private readonly ArrayRowBuffer _rowBuffer;
    private readonly CombinedRowBuffer _combinedRowBuffer;

    public EmittedComputeScalarIterator(Iterator input, Action<RowBuffer, ArrayRowBuffer> writer, RowBufferLayout computedLayout, RowBuffer? outer)
    {
        _input = input;
        _writer = writer;
        _functionRowBuffer = outer is null ? input.RowBuffer : new CombinedRowBuffer(outer, input.RowBuffer);
        _rowBuffer = new ArrayRowBuffer(computedLayout);
        _combinedRowBuffer = new CombinedRowBuffer(input.RowBuffer, _rowBuffer);
    }

    public override RowBuffer RowBuffer => _combinedRowBuffer;

    public override void Open() => _input.Open();

    public override void Dispose() => _input.Dispose();

    public override bool Read()
    {
        if (!_input.Read())
            return false;

        _writer(_functionRowBuffer, _rowBuffer);

        return true;
    }
}
