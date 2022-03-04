using System.Collections;
using System.Collections.Immutable;

using NQuery.Symbols.Aggregation;

namespace NQuery.Iterators
{
    internal sealed class StreamAggregateIterator : Iterator
    {
        private readonly Iterator _input;
        private readonly ImmutableArray<IComparer> _comparers;
        private readonly ImmutableArray<RowBufferEntry> _groupEntries;
        private readonly ImmutableArray<IAggregator> _aggregators;
        private readonly ImmutableArray<IteratorFunction> _argumentFunctions;
        private readonly ArrayRowBuffer _rowBuffer;

        private bool _eof;
        private bool _isFirstRecord;

        public StreamAggregateIterator(Iterator input, IEnumerable<RowBufferEntry> groupEntries, ImmutableArray<IComparer> comparers, IEnumerable<IAggregator> aggregators, IEnumerable<IteratorFunction> argumentFunctions)
        {
            _input = input;
            _comparers = comparers;
            _groupEntries = groupEntries.ToImmutableArray();
            _aggregators = aggregators.ToImmutableArray();
            _argumentFunctions = argumentFunctions.ToImmutableArray();
            _rowBuffer = new ArrayRowBuffer(_groupEntries.Length + _aggregators.Length);
        }

        public override RowBuffer RowBuffer
        {
            get { return _rowBuffer; }
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
                var aggregator = _aggregators[i];
                var argument = _argumentFunctions[i]();
                aggregator.Accumulate(argument);
            }
        }

        private void TerminatedAggregates()
        {
            for (var i = 0; i < _aggregators.Length; i++)
            {
                var aggregator = _aggregators[i];
                var result = aggregator.GetResult();
                _rowBuffer.Array[_groupEntries.Length + i] = result;
            }
        }

        private void FillGroupByExpressions(object[] target)
        {
            for (var i = 0; i < _groupEntries.Length; i++)
            {
                target[i] = _groupEntries[i].GetValue();
            }
        }

        private bool CheckIfCurrentRowIsInSameGroup()
        {
            // Assume we are in the same group until proven otherwise.
            var inSameGroup = true;

            // We have a last one row (which means the current row in _rowBuffer is
            // not the first record).
            //
            // Compare this GROUP BY values with the last ones.

            for (var i = 0; i < _groupEntries.Length; i++)
            {
                var valueOfLastRow = _rowBuffer[i];
                var valueOfThisRow = _groupEntries[i].GetValue();
                var comparer = _comparers[i];
                var equalsPreviousRow = comparer.Compare(valueOfLastRow, valueOfThisRow) == 0;

                if (!equalsPreviousRow)
                {
                    // They are not equal. This means the current row in the row buffer
                    // belongs to another group.
                    inSameGroup = false;
                    break;
                }
            }

            return inSameGroup;
        }

        public override void Open()
        {
            _input.Open();
            _eof = !_input.Read();
            _isFirstRecord = true;
        }

        public override void Dispose()
        {
            _input.Dispose();
        }

        public override bool Read()
        {
            if (_eof)
            {
                if (_groupEntries.Length == 0 && _isFirstRecord)
                {
                    _isFirstRecord = false;
                    InitializeAggregates();
                    TerminatedAggregates();
                    return true;
                }

                return false;
            }

            _isFirstRecord = false;

            InitializeAggregates();
            FillGroupByExpressions(_rowBuffer.Array);
            do
            {
                AccumulateAggregates();

                // Next one please.
                if (!_input.Read())
                {
                    _eof = true;
                    break;
                }
            }
            while (CheckIfCurrentRowIsInSameGroup());

            TerminatedAggregates();

            return true;
        }
    }
}