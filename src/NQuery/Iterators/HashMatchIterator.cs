using NQuery.Binding;

namespace NQuery.Iterators
{
    internal sealed class HashMatchIterator : Iterator
    {
        private readonly BoundHashMatchOperator _logicalOperator;
        private readonly Iterator _build;
        private readonly Iterator _probe;
        private readonly int _buildIndex;
        private readonly int _probeIndex;
        private readonly IteratorPredicate _remainder;
        private readonly HashMatchRowBuffer _rowBuffer;

        private Dictionary<object, HashMatchEntry> _hashTable;
        private HashMatchEntry _entry;
        private IEnumerator<HashMatchEntry> _entryEnumerator;
        private Phase _currentPhase;
        private bool _probeMatched;

        private static readonly object NullKey = new object();

        public HashMatchIterator(BoundHashMatchOperator logicalOperator, Iterator build, Iterator probe, int buildIndex, int probeIndex, IteratorPredicate remainder, HashMatchRowBuffer rowBuffer)
        {
            _logicalOperator = logicalOperator;
            _build = build;
            _probe = probe;
            _buildIndex = buildIndex;
            _probeIndex = probeIndex;
            _remainder = remainder;
            _rowBuffer = rowBuffer;
        }

        public override RowBuffer RowBuffer
        {
            get { return _rowBuffer; }
        }

        public override void Open()
        {
            _build.Open();
            _probe.Open();
            BuildHashtable();

            _entry = null;
            _entryEnumerator = null;
            _currentPhase = Phase.ProduceMatch;
            _probeMatched = true;
        }

        public override void Dispose()
        {
            _build.Dispose();
            _probe.Dispose();
        }

        private void BuildHashtable()
        {
            _hashTable = new Dictionary<object, HashMatchEntry>();

            while (_build.Read())
            {
                var keyValue = _build.RowBuffer[_buildIndex] ?? NullKey;
                var rowValues = new object[_build.RowBuffer.Count];
                _build.RowBuffer.CopyTo(rowValues, 0);
                AddToHashtable(keyValue, rowValues);
            }
        }

        private void AddToHashtable(object keyValue, object[] values)
        {
            HashMatchEntry entry;
            _hashTable.TryGetValue(keyValue, out entry);

            if (entry is null)
            {
                entry = new HashMatchEntry();
            }
            else
            {
                var newEntry = new HashMatchEntry { Next = entry };
                entry = newEntry;
            }

            entry.RowValues = values;
            _hashTable[keyValue] = entry;
        }

        public override bool Read()
        {
            switch (_currentPhase)
            {
                case Phase.ProduceMatch:
                {
                    var matchFound = false;
                    _rowBuffer.SetProbe(_probe.RowBuffer);

                    while (!matchFound)
                    {
                        if (_entry is not null)
                            _entry = _entry.Next;

                        if (_entry is null)
                        {
                            // All rows having the same key value are exhausted.

                            if (!_probeMatched && (_logicalOperator == BoundHashMatchOperator.FullOuter ||
                                                   _logicalOperator == BoundHashMatchOperator.RightOuter))
                            {
                                _probeMatched = true;
                                _rowBuffer.SetBuild(null);
                                return true;
                            }

                            // Read next row from probe input.

                            if (!_probe.Read())
                            {
                                // The probe input is exhausted. If we have a full outer or left outer
                                // join we are not finished. We have to return all rows from the build
                                // input that have not been matched with the probe input.

                                if (_logicalOperator == BoundHashMatchOperator.FullOuter ||
                                    _logicalOperator == BoundHashMatchOperator.LeftOuter)
                                {
                                    _currentPhase = Phase.ReturnUnmatchedRowsFromBuildInput;
                                    _entry = null;
                                    goto case Phase.ReturnUnmatchedRowsFromBuildInput;
                                }

                                return false;
                            }

                            // Get probe value

                            _probeMatched = false;
                            var probeValue = _probe.RowBuffer[_probeIndex];

                            // Seek first occurrence of probe value

                            if (probeValue is not null)
                                _hashTable.TryGetValue(probeValue, out _entry);
                        }

                        if (_entry is not null)
                        {
                            _rowBuffer.SetBuild(_entry);

                            if (_remainder())
                            {
                                _entry.Matched = true;
                                matchFound = true;
                                _probeMatched = true;
                            }
                        }
                    }

                    return true;
                }

                case Phase.ReturnUnmatchedRowsFromBuildInput:
                {
                    var unmatchedFound = false;
                    _rowBuffer.SetProbe(null);

                    while (!unmatchedFound)
                    {
                        if (_entry is not null)
                            _entry = _entry.Next;

                        if (_entry is null)
                        {
                            if (_entryEnumerator is null)
                                _entryEnumerator = _hashTable.Values.GetEnumerator();

                            // All rows having the same key value are exhausted.
                            // Read next key from build input.

                            if (!_entryEnumerator.MoveNext())
                            {
                                // We have read all keys. So we are finished.
                                return false;
                            }

                            _entry = _entryEnumerator.Current;
                        }

                        unmatchedFound = !_entry.Matched;
                    }

                    _rowBuffer.SetBuild(_entry);
                    return true;
                }

                default:
                    throw ExceptionBuilder.UnexpectedValue(_currentPhase);
            }
        }

        private enum Phase
        {
            ProduceMatch,
            ReturnUnmatchedRowsFromBuildInput
        }
    }
}