using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Iterators;

// Computes its columns by running the compiled writers over the input row, storing
// each result typed into its own appended buffer. The writers take the row buffer as a
// parameter, so they are compiled once and shared across executions.
//
// When this compute is the correlated part of an Apply's right side, its writers
// reference the outer (left) row. The outer buffer is then prepended to the input
// buffer (matching the (outer ++ input) slot layout the writers were compiled
// against); the computed columns are still appended to the input's own columns.
internal sealed class EmittedComputeScalarIterator : Iterator
{
    private readonly Iterator _input;
    private readonly ImmutableArray<Action<RowBuffer, ArrayRowBuffer>> _writers;
    private readonly RowBuffer _functionRowBuffer;
    private readonly ArrayRowBuffer _rowBuffer;
    private readonly CombinedRowBuffer _combinedRowBuffer;

    public EmittedComputeScalarIterator(Iterator input, ImmutableArray<Action<RowBuffer, ArrayRowBuffer>> writers, RowBufferLayout computedLayout, RowBuffer? outer)
    {
        _input = input;
        _writers = writers;
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

        foreach (var writer in _writers)
            writer(_functionRowBuffer, _rowBuffer);

        return true;
    }
}
