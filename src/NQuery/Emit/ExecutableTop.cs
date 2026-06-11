#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    internal sealed class ExecutableTop : ExecutableOperator
    {
        private readonly ExecutableOperator _input;
        private readonly int _limit;
        private readonly ImmutableArray<BoundComparedValue> _tieEntries;

        public ExecutableTop(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, int limit, ImmutableArray<BoundComparedValue> tieEntries)
            : base(outputValueSlots)
        {
            _input = input;
            _limit = limit;
            _tieEntries = tieEntries;
        }

        public override Iterator CreateIterator(RowBuffer? outer)
        {
            var input = _input.CreateIterator(outer);
            if (_tieEntries.IsEmpty)
                return new TopIterator(input, _limit);

            var allocation = Allocate(_input, input);
            var tieEntries = _tieEntries.Select(t => allocation[t.ValueSlot]).ToImmutableArray();
            var tieComparers = _tieEntries.Select(t => t.Comparer).ToImmutableArray();
            return new TopWithTiesIterator(input, _limit, tieEntries, tieComparers);
        }
    }
}
