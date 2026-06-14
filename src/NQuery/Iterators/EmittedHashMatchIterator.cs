#nullable enable

namespace NQuery.Iterators
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
    //
    // semi / anti carry the existence-join semantics. They consume the probe purely to
    // mark which build rows matched (no joined row is produced) and then output the build
    // side only: semi emits the build rows that matched, anti the ones that did not. A
    // probing semi (probe set) instead emits *every* build row with a trailing boolean
    // reporting whether it matched -- the decorrelated form of EXISTS, whose enclosing
    // filter then tests that boolean.
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
        private readonly bool _semi;
        private readonly bool _anti;
        private readonly HashMatchRowBuffer _rowBuffer;
        private readonly RowBuffer _outputRowBuffer;
        private readonly ProbedRowBuffer? _probedRowBuffer;

        private Dictionary<object, HashMatchEntry> _hashTable = null!;
        private List<HashMatchEntry> _buildRows = null!;
        private HashMatchEntry? _entry;
        private int _flushIndex;
        private Phase _currentPhase;
        private bool _probeMatched;

        public EmittedHashMatchIterator(Iterator build, Iterator probe, int buildIndex, int probeIndex, EmittedPredicate remainder, bool preserveBuild, bool preserveProbe, bool semi = false, bool anti = false, bool probing = false)
        {
            _build = build;
            _probe = probe;
            _buildIndex = buildIndex;
            _probeIndex = probeIndex;
            _remainder = remainder;
            _preserveBuild = preserveBuild;
            _preserveProbe = preserveProbe;
            _semi = semi;
            _anti = anti;
            _rowBuffer = new HashMatchRowBuffer(build.RowBuffer.Count, probe.RowBuffer.Count);

            // Inner/outer output the combined row; semi/anti output the build side only,
            // with a probing semi appending its boolean match-marker column.
            if (semi || anti)
            {
                if (probing)
                {
                    _probedRowBuffer = new ProbedRowBuffer(_rowBuffer.Build);
                    _outputRowBuffer = _probedRowBuffer;
                }
                else
                {
                    _outputRowBuffer = _rowBuffer.Build;
                }
            }
            else
            {
                _outputRowBuffer = _rowBuffer;
            }
        }

        public override RowBuffer RowBuffer => _outputRowBuffer;

        // Whether a probe-time match yields a joined output row. Semi/anti consume the
        // probe only to mark matches; their output comes entirely from the build flush.
        private bool EmitMatches => !_semi && !_anti;

        // Whether the post-probe pass over the build rows runs: to flush unmatched build
        // rows (left/full outer), or to emit the build rows a semi/anti existence test
        // selected.
        private bool FlushBuild => _preserveBuild || _semi || _anti;

        public override void Open()
        {
            _build.Open();
            _probe.Open();
            BuildHashtable();

            _entry = null;
            _flushIndex = 0;
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
            _buildRows = new List<HashMatchEntry>();

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
            _hashTable.TryGetValue(keyValue, out var existing);
            var entry = new HashMatchEntry { RowValues = values, Next = existing };
            _hashTable[keyValue] = entry;

            // Keep the build rows in scan order as well; the flush pass walks this list (not
            // the hash table's unordered values) so unmatched/semi/anti rows preserve the
            // build input's order.
            _buildRows.Add(entry);
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
                            if (EmitMatches && !_probeMatched && _preserveProbe)
                            {
                                _probeMatched = true;
                                _rowBuffer.SetBuild(null);
                                return true;
                            }

                            if (!_probe.Read())
                            {
                                // Probe exhausted. A left/full outer flushes its unmatched
                                // build rows; a semi/anti now emits its selected build rows.
                                if (FlushBuild)
                                {
                                    _currentPhase = Phase.FlushBuildInput;
                                    _entry = null;
                                    goto case Phase.FlushBuildInput;
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
                                _probeMatched = true;

                                // Semi/anti keep scanning to mark every matched build row;
                                // only an inner/outer match produces a row here.
                                if (EmitMatches)
                                    matchFound = true;
                            }
                        }
                    }

                    return true;
                }

                case Phase.FlushBuildInput:
                {
                    _rowBuffer.SetProbe(null);

                    while (_flushIndex < _buildRows.Count)
                    {
                        var entry = _buildRows[_flushIndex++];

                        // A probing semi emits every build row (the marker, set below, carries
                        // whether it matched). A plain semi emits the matched build rows; anti
                        // and left/full outer emit the unmatched ones.
                        var emit = _probedRowBuffer is not null || (_semi ? entry.Matched : !entry.Matched);
                        if (!emit)
                            continue;

                        _rowBuffer.SetBuild(entry);
                        _probedRowBuffer?.SetProbe(entry.Matched);
                        return true;
                    }

                    return false;
                }

                default:
                    throw ExceptionBuilder.UnexpectedValue(_currentPhase);
            }
        }

        private enum Phase
        {
            ProduceMatch,
            FlushBuildInput
        }
    }
}
