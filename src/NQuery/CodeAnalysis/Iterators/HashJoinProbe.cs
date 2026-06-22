namespace NQuery.CodeAnalysis.Iterators;

// The hash table behind HashMatchIterator: it maps each build row's join key to the head of
// a chain of build rows sharing that key, and resolves a probe row's key to that head. The
// key used to be read boxed into a Dictionary<object, int> (with a sentinel for NULL); this
// reads it unboxed into a Dictionary<T, int> for the value-typed kinds, so building and
// probing the table no longer box every key.
//
// A NULL key never matches (SQL equality), so it is simply not inserted -- the probe side
// returns NoEntry for a NULL key and never finds one. The per-row chain (_next) still gets
// an entry for every build row (including NULL-keyed ones) so it stays index-aligned with
// the build store, which the iterator's flush pass walks by row index.
internal abstract class HashJoinProbe
{
    public const int NoEntry = -1;

    protected readonly List<int> Next = new();

    // Registers a build row, reading its key from the build buffer's current row.
    public abstract void Add(int row);

    // The head entry for the probe buffer's current key, or NoEntry for a NULL/absent key.
    public abstract int GetHead();

    // The next build row in the chain after the given entry, or NoEntry at the chain's end.
    public int GetNext(int entry) => Next[entry];

    public virtual void Clear() => Next.Clear();

    public static HashJoinProbe Create(RowBufferEntry buildKey, RowBufferEntry probeKey)
    {
        // The join is equi-keyed on a single type, so the two sides share a kind and type;
        // the build side drives the dispatch.
        if (buildKey.Column.Kind == RowBufferColumnKind.Object)
            return new ObjectHashJoinProbe(buildKey, probeKey);

        var probeType = typeof(TypedHashJoinProbe<>).MakeGenericType(buildKey.Type);
        return (HashJoinProbe)Activator.CreateInstance(probeType, buildKey, probeKey)!;
    }
}

// A value-typed join key: the hash table is a Dictionary<T, int>, so keys are stored and
// looked up unboxed. EqualityComparer<T>.Default decides key equality -- value equality for
// the bit-packed types, matching the boxed Dictionary<object, int> it replaces.
internal sealed class TypedHashJoinProbe<T> : HashJoinProbe
    where T : struct
{
    private readonly Dictionary<T, int> _table = new();
    private readonly RowBuffer _buildBuffer;
    private readonly RowBufferColumn _buildColumn;
    private readonly RowBuffer _probeBuffer;
    private readonly RowBufferColumn _probeColumn;

    public TypedHashJoinProbe(RowBufferEntry buildKey, RowBufferEntry probeKey)
    {
        _buildBuffer = buildKey.RowBuffer;
        _buildColumn = buildKey.Column;
        _probeBuffer = probeKey.RowBuffer;
        _probeColumn = probeKey.Column;
    }

    public override void Add(int row)
    {
        var key = _buildBuffer.ReadValue<T>(_buildColumn);
        if (!key.HasValue)
        {
            // NULL never matches, so it is not inserted; the chain entry keeps row alignment.
            Next.Add(NoEntry);
            return;
        }

        var value = key.GetValueOrDefault();
        Next.Add(_table.TryGetValue(value, out var existing) ? existing : NoEntry);
        _table[value] = row;
    }

    public override int GetHead()
    {
        var key = _probeBuffer.ReadValue<T>(_probeColumn);
        if (!key.HasValue)
            return NoEntry;

        return _table.TryGetValue(key.GetValueOrDefault(), out var head) ? head : NoEntry;
    }

    public override void Clear()
    {
        base.Clear();
        _table.Clear();
    }
}

// A reference-typed (object container) join key: the values are already object, so the table
// stays a Dictionary<object, int> keyed on them directly, as before.
internal sealed class ObjectHashJoinProbe : HashJoinProbe
{
    private readonly Dictionary<object, int> _table = new();
    private readonly RowBuffer _buildBuffer;
    private readonly int _buildIndex;
    private readonly RowBuffer _probeBuffer;
    private readonly int _probeIndex;

    public ObjectHashJoinProbe(RowBufferEntry buildKey, RowBufferEntry probeKey)
    {
        _buildBuffer = buildKey.RowBuffer;
        _buildIndex = buildKey.Column.Index;
        _probeBuffer = probeKey.RowBuffer;
        _probeIndex = probeKey.Column.Index;
    }

    public override void Add(int row)
    {
        var key = _buildBuffer.GetObject(_buildIndex);
        if (key is null)
        {
            Next.Add(NoEntry);
            return;
        }

        Next.Add(_table.TryGetValue(key, out var existing) ? existing : NoEntry);
        _table[key] = row;
    }

    public override int GetHead()
    {
        var key = _probeBuffer.GetObject(_probeIndex);
        if (key is null)
            return NoEntry;

        return _table.TryGetValue(key, out var head) ? head : NoEntry;
    }

    public override void Clear()
    {
        base.Clear();
        _table.Clear();
    }
}
