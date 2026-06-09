#nullable enable

using System.Collections.Immutable;

namespace NQuery.EmittedIterators
{
    // Like ComputeScalarIterator, but its functions take the input row buffer as a
    // parameter, so the compiled functions are shared across executions.
    internal sealed class EmittedComputeScalarIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly ImmutableArray<EmittedFunction> _functions;
        private readonly ArrayRowBuffer _rowBuffer;
        private readonly CombinedRowBuffer _combinedRowBuffer;

        public EmittedComputeScalarIterator(Iterator input, ImmutableArray<EmittedFunction> functions)
        {
            _input = input;
            _functions = functions;
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
                _rowBuffer.Array[i] = _functions[i](_input.RowBuffer);

            return true;
        }
    }
}
