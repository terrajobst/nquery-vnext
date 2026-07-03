namespace NQuery.CodeAnalysis.Iterators;

// A lazy index spool: the runtime of a correlated equality filter over an
// outer-independent input. The first Open drains the input once into a columnar
// store and indexes it by the key column; every Open then looks up the current
// outer probe value and Read walks just the matching rows. The input is never
// re-opened -- its content cannot change within one execution, which is exactly
// the independence condition the planner verified before choosing this operator.
//
// The index is a HashJoinProbe, shared with the hash match: keys are read unboxed
// for the value-typed kinds (a Dictionary<T, int> rather than Dictionary<object, ...>)
// and the matching rows are threaded through an intrusive chain instead of a per-key
// list. Equality semantics match the filter it replaces: a NULL key never equals
// anything, so NULL-keyed rows are not indexed (they sit in the store, off every
// chain) and a NULL probe matches nothing. Chain order is newest-first, which the
// enclosing correlated join does not depend on.
internal sealed class IndexSpoolIterator : Iterator
{
    private readonly Iterator _input;
    private readonly HashJoinProbe _index;
    private readonly SpooledRowStore _store;
    private readonly SpooledRowStore.Cursor _cursor;

    private bool _built;
    private int _entry;

    public IndexSpoolIterator(Iterator input, RowBufferEntry indexEntry, RowBufferEntry probeEntry)
    {
        ThrowIfNull(input);

        _input = input;
        _index = HashJoinProbe.Create(indexEntry, probeEntry);
        _store = new SpooledRowStore(input.RowBuffer);
        _cursor = _store.CreateCursor();
    }

    public override RowBuffer RowBuffer => _cursor;

    public override void Open()
    {
        if (!_built)
        {
            BuildIndex();
            _built = true;
        }

        _entry = _index.GetHead();
    }

    private void BuildIndex()
    {
        // Every row is stored so the chain (which carries a Next entry per row) stays
        // index-aligned with the store; a NULL-keyed row is stored but left off its chain.
        _input.Open();
        while (_input.Read())
        {
            var row = _store.Count;
            _store.Append(_input.RowBuffer);
            _index.Add(row);
        }
    }

    public override void Dispose()
    {
        _input.Dispose();
    }

    public override bool Read()
    {
        if (_entry == HashJoinProbe.NoEntry)
            return false;

        _cursor.Row = _entry;
        _entry = _index.GetNext(_entry);
        return true;
    }
}
