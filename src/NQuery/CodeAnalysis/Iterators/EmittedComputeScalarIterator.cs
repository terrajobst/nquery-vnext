using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Iterators;

// Like ComputeScalarIterator, but its functions take the input row buffer as a
// parameter, so the compiled functions are shared across executions.
//
// When this compute is the correlated part of an Apply's right side, its functions
// reference the outer (left) row. The outer buffer is then prepended to the input
// buffer (matching the (outer ++ input) slot layout the functions were compiled
// against); the computed columns are still appended to the input's own columns.
internal sealed class EmittedComputeScalarIterator : Iterator
{
    private readonly Iterator _input;
    private readonly ImmutableArray<EmittedFunction> _functions;
    private readonly RowBuffer _functionRowBuffer;
    private readonly ArrayRowBuffer _rowBuffer;
    private readonly CombinedRowBuffer _combinedRowBuffer;

    public EmittedComputeScalarIterator(Iterator input, ImmutableArray<EmittedFunction> functions, RowBuffer? outer)
    {
        _input = input;
        _functions = functions;
        _functionRowBuffer = outer is null ? input.RowBuffer : new CombinedRowBuffer(outer, input.RowBuffer);
        _rowBuffer = new ArrayRowBuffer(functions.Length);
        _combinedRowBuffer = new CombinedRowBuffer(input.RowBuffer, _rowBuffer);
    }

    public override RowBuffer RowBuffer => _combinedRowBuffer;

    public override void Open() => _input.Open();

    public override void Dispose() => _input.Dispose();

    public override bool Read()
    {
        if (!_input.Read())
            return false;

        for (var i = 0; i < _functions.Length; i++)
            _rowBuffer.Array[i] = _functions[i](_functionRowBuffer);

        return true;
    }
}
