namespace NQuery.CodeAnalysis.Iterators;

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
internal sealed class HashMatchIterator : Iterator
{
    private static readonly object NullKey = new();

    private readonly Iterator _build;
    private readonly Iterator _probe;
    private readonly RowBufferEntry _buildKey;
    private readonly RowBufferEntry _probeKey;
    private readonly CompiledPredicate _remainder;
    private readonly bool _preserveBuild;
    private readonly bool _preserveProbe;
    private readonly bool _semi;
    private readonly bool _anti;
    private readonly HashMatchRowBuffer _rowBuffer;
    private readonly RowBuffer _remainderRowBuffer;
    private readonly RowBuffer _outputRowBuffer;
    private readonly ProbedRowBuffer? _probedRowBuffer;

    // The build side is spooled columnar: one set of backing arrays for all build rows
    // (see SpooledRowStore) instead of an ArrayRowBuffer clone per row. A build row is an
    // index; _buildCursor is a single reusable view repositioned onto the current row.
    private readonly SpooledRowStore _buildStore;
    private readonly SpooledRowStore.Cursor _buildCursor;

    private const int NoEntry = -1;

    private Dictionary<object, int> _hashTable = null!;
    private List<int> _next = null!;
    private bool[] _matched = null!;
    private int _entry;
    private int _flushIndex;
    private Phase _currentPhase;
    private bool _probeMatched;

    public HashMatchIterator(Iterator build, Iterator probe, RowBufferEntry buildKey, RowBufferEntry probeKey, CompiledPredicate remainder, bool preserveBuild, bool preserveProbe, bool semi = false, bool anti = false, bool probing = false, RowBuffer? outer = null)
    {
        _build = build;
        _probe = probe;
        _buildKey = buildKey;
        _probeKey = probeKey;
        _remainder = remainder;
        _preserveBuild = preserveBuild;
        _preserveProbe = preserveProbe;
        _semi = semi;
        _anti = anti;
        _buildStore = new SpooledRowStore(build.RowBuffer);
        _buildCursor = _buildStore.CreateCursor();
        _rowBuffer = new HashMatchRowBuffer(build.RowBuffer, probe.RowBuffer);

        // When this hash match is correlated (inside an Apply's right side), its remainder
        // references the outer row. The outer buffer is prepended to the (build ++ probe)
        // buffer, matching the (outer ++ build ++ probe) layout the remainder was compiled
        // against; the rows this iterator exposes are unchanged.
        _remainderRowBuffer = outer is null ? _rowBuffer : new CombinedRowBuffer(outer, _rowBuffer);

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

        _entry = NoEntry;
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
        _buildStore.Clear();
        _hashTable = new Dictionary<object, int>();
        _next = new List<int>();

        while (_build.Read())
        {
            var keyValue = _buildKey.GetValue() ?? NullKey;
            var row = _buildStore.Count;
            _buildStore.Append(_build.RowBuffer);
            AddToHashtable(keyValue, row);
        }

        _matched = _buildStore.Count == 0 ? Array.Empty<bool>() : new bool[_buildStore.Count];
    }

    // Chains the new build row at the head of its key's bucket (so a bucket lists its rows
    // newest-first); _next[row] points at the previous head, NoEntry terminating the chain.
    // The flush pass walks rows in scan order (index order), not the chain, so unmatched/
    // semi/anti rows preserve the build input's order.
    private void AddToHashtable(object keyValue, int row)
    {
        _next.Add(_hashTable.TryGetValue(keyValue, out var existing) ? existing : NoEntry);
        _hashTable[keyValue] = row;
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
                    _entry = _entry == NoEntry ? NoEntry : _next[_entry];

                    if (_entry == NoEntry)
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
                                _entry = NoEntry;
                                goto case Phase.FlushBuildInput;
                            }

                            return false;
                        }

                        _probeMatched = false;
                        var probeValue = _probeKey.GetValue();
                        if (probeValue is not null && _hashTable.TryGetValue(probeValue, out var head))
                            _entry = head;
                    }

                    if (_entry != NoEntry)
                    {
                        _buildCursor.Row = _entry;
                        _rowBuffer.SetBuild(_buildCursor);

                        if (_remainder(_remainderRowBuffer))
                        {
                            _matched[_entry] = true;
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

                while (_flushIndex < _buildStore.Count)
                {
                    var row = _flushIndex++;

                    // A probing semi emits every build row (the marker, set below, carries
                    // whether it matched). A plain semi emits the matched build rows; anti
                    // and left/full outer emit the unmatched ones.
                    var emit = _probedRowBuffer is not null || (_semi ? _matched[row] : !_matched[row]);
                    if (!emit)
                        continue;

                    _buildCursor.Row = row;
                    _rowBuffer.SetBuild(_buildCursor);
                    _probedRowBuffer?.SetProbe(_matched[row]);
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
