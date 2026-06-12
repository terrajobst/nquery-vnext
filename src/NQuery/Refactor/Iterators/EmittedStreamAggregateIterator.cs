#nullable enable

using System.Collections;
using System.Collections.Immutable;

using NQuery.Symbols.Aggregation;

namespace NQuery.Refactor.Iterators
{
    // Stream aggregate: collapses runs of consecutive equal-group rows into one output
    // row. It relies on the input arriving sorted on the grouping columns (the planner
    // inserts a sort to guarantee that), so it only ever holds a single group's state.
    //
    // Like the other emitted iterators, the argument functions take the row buffer as a
    // parameter and are compiled once. When this aggregate is the correlated part of an
    // Apply's right side, those functions reference the outer (left) row, so the outer
    // buffer is prepended to the input buffer -- matching the (outer ++ input) slot
    // layout the functions and group indices were resolved against.
    //
    // The output row buffer lays out the group values first, then the aggregate
    // results, matching PhysicalStreamAggregates's output slot order.
    internal sealed class EmittedStreamAggregateIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly ImmutableArray<int> _groupIndices;
        private readonly ImmutableArray<IComparer> _comparers;
        private readonly ImmutableArray<IAggregator> _aggregators;
        private readonly ImmutableArray<EmittedFunction> _argumentFunctions;
        private readonly RowBuffer _readRowBuffer;
        private readonly ArrayRowBuffer _rowBuffer;

        private bool _eof;
        private bool _isFirstRecord;

        public EmittedStreamAggregateIterator(Iterator input, ImmutableArray<int> groupIndices, ImmutableArray<IComparer> comparers, ImmutableArray<IAggregator> aggregators, ImmutableArray<EmittedFunction> argumentFunctions, RowBuffer? outer)
        {
            _input = input;
            _groupIndices = groupIndices;
            _comparers = comparers;
            _aggregators = aggregators;
            _argumentFunctions = argumentFunctions;
            _readRowBuffer = outer is null ? input.RowBuffer : new CombinedRowBuffer(outer, input.RowBuffer);
            _rowBuffer = new ArrayRowBuffer(groupIndices.Length + aggregators.Length);
        }

        public override RowBuffer RowBuffer => _rowBuffer;

        public override void Open()
        {
            _input.Open();
            _eof = !_input.Read();
            _isFirstRecord = true;
        }

        public override void Dispose() => _input.Dispose();

        public override bool Read()
        {
            if (_eof)
            {
                // With no GROUP BY, an empty input still yields a single row holding the
                // aggregates over zero rows (e.g. SELECT COUNT(*) returns 0).
                if (_groupIndices.Length == 0 && _isFirstRecord)
                {
                    _isFirstRecord = false;
                    InitializeAggregates();
                    StoreAggregates();
                    return true;
                }

                return false;
            }

            _isFirstRecord = false;

            InitializeAggregates();
            StoreGroupValues();
            do
            {
                AccumulateAggregates();

                if (!_input.Read())
                {
                    _eof = true;
                    break;
                }
            }
            while (IsCurrentRowInSameGroup());

            StoreAggregates();
            return true;
        }

        private void InitializeAggregates()
        {
            foreach (var aggregator in _aggregators)
                aggregator.Initialize();
        }

        private void AccumulateAggregates()
        {
            for (var i = 0; i < _aggregators.Length; i++)
            {
                var argument = _argumentFunctions[i](_readRowBuffer);
                _aggregators[i].Accumulate(argument);
            }
        }

        private void StoreAggregates()
        {
            for (var i = 0; i < _aggregators.Length; i++)
                _rowBuffer.Array[_groupIndices.Length + i] = _aggregators[i].GetResult();
        }

        private void StoreGroupValues()
        {
            for (var i = 0; i < _groupIndices.Length; i++)
                _rowBuffer.Array[i] = _readRowBuffer[_groupIndices[i]];
        }

        private bool IsCurrentRowInSameGroup()
        {
            // The previously seen group key sits in the output buffer's leading columns;
            // compare it against the current input row's grouping values.
            for (var i = 0; i < _groupIndices.Length; i++)
            {
                var previous = _rowBuffer[i];
                var current = _readRowBuffer[_groupIndices[i]];
                if (_comparers[i].Compare(previous, current) != 0)
                    return false;
            }

            return true;
        }
    }
}
