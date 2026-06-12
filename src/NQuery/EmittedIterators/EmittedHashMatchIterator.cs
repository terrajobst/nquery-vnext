#nullable enable

namespace NQuery.EmittedIterators
{
    // Hash match (hash join) over an equi-key. The build input is consumed in full into
    // a hash table keyed by the build key; the probe input is then streamed, each row
    // looking up its probe key and joining against the chained build rows. The compiled
    // remainder predicate (the non-equi residual of the join condition) runs over the
    // combined build ++ probe row; like the other emitted iterators it takes the row
    // buffer as a parameter, so it is compiled once and reused.
    //
    // preserveBuild / preserveProbe carry the outer-join semantics: an unmatched build
    // row is emitted (probe NULL-padded) when preserveBuild is set, and an unmatched
    // probe row is emitted (build NULL-padded) when preserveProbe is set. Inner = neither,
    // left outer = build, full outer = both. A NULL key never matches (SQL equality), so
    // a NULL-keyed row is unmatched and surfaces only through the outer paths.
    internal sealed class EmittedHashMatchIterator : Iterator
    {
        private static readonly object NullKey = new();

        private readonly Iterator _build;
        private readonly Iterator _probe;
        private readonly int _buildIndex;
        private readonly int _probeIndex;
        private readonly EmittedPredicate _remainder;
        private readonly bool _preserveBuild;
        private readonly bool _preserveProbe;
        private readonly HashMatchRowBuffer _rowBuffer;

        private Dictionary<object, HashMatchEntry> _hashTable = null!;
        private HashMatchEntry? _entry;
        private IEnumerator<HashMatchEntry>? _entryEnumerator;
        private Phase _currentPhase;
        private bool _probeMatched;

        public EmittedHashMatchIterator(Iterator build, Iterator probe, int buildIndex, int probeIndex, EmittedPredicate remainder, bool preserveBuild, bool preserveProbe)
        {
            _build = build;
            _probe = probe;
            _buildIndex = buildIndex;
            _probeIndex = probeIndex;
            _remainder = remainder;
            _preserveBuild = preserveBuild;
            _preserveProbe = preserveProbe;
            _rowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);
        }

        public override RowBuffer RowBuffer => _rowBuffer;

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
            _hashTable.TryGetValue(keyValue, out var entry);
            entry = entry is null ? new HashMatchEntry() : new HashMatchEntry { Next = entry };
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
                        _entry = _entry?.Next;

                        if (_entry is null)
                        {
                            // The chain for the current probe key is exhausted. An
                            // unmatched probe row is an output for a full outer join.
                            if (!_probeMatched && _preserveProbe)
                            {
                                _probeMatched = true;
                                _rowBuffer.SetBuild(null);
                                return true;
                            }

                            if (!_probe.Read())
                            {
                                // Probe exhausted. A left/full outer must still flush the
                                // build rows that were never matched.
                                if (_preserveBuild)
                                {
                                    _currentPhase = Phase.ReturnUnmatchedRowsFromBuildInput;
                                    _entry = null;
                                    goto case Phase.ReturnUnmatchedRowsFromBuildInput;
                                }

                                return false;
                            }

                            _probeMatched = false;
                            var probeValue = _probe.RowBuffer[_probeIndex];
                            if (probeValue is not null)
                                _hashTable.TryGetValue(probeValue, out _entry);
                        }

                        if (_entry is not null)
                        {
                            _rowBuffer.SetBuild(_entry);

                            if (_remainder(_rowBuffer))
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
                        _entry = _entry?.Next;

                        if (_entry is null)
                        {
                            _entryEnumerator ??= _hashTable.Values.GetEnumerator();

                            if (!_entryEnumerator.MoveNext())
                                return false;

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
