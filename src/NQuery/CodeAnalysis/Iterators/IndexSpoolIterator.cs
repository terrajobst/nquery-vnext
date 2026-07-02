namespace NQuery.CodeAnalysis.Iterators;

// A lazy index spool: the runtime of a correlated equality filter over an
// outer-independent input. The first Open drains the input once into a columnar
// store and builds a hash index over the key column; every Open then looks up the
// current outer probe value and Read walks just the matching rows. The input is
// never re-opened -- its content cannot change within one execution, which is
// exactly the independence condition the planner verified before choosing this
// operator.
//
// Equality semantics match the filter it replaces: a NULL key never equals
// anything, so NULL-keyed rows are not indexed at all and a NULL probe matches no
// rows. Key comparison is the boxed object.Equals/GetHashCode, the same contract
// the hash match's build table relies on.
internal sealed class IndexSpoolIterator : Iterator
{
    private readonly Iterator _input;
    private readonly RowBufferEntry _indexEntry;
    private readonly RowBufferEntry _probeEntry;
    private readonly SpooledRowStore _store;
    private readonly SpooledRowStore.Cursor _cursor;

    private Dictionary<object, List<int>>? _index;
    private List<int>? _matches;
    private int _position;

    public IndexSpoolIterator(Iterator input, RowBufferEntry indexEntry, RowBufferEntry probeEntry)
    {
        _input = input;
        _indexEntry = indexEntry;
        _probeEntry = probeEntry;
        _store = new SpooledRowStore(input.RowBuffer);
        _cursor = _store.CreateCursor();
    }

    public override RowBuffer RowBuffer => _cursor;

    public override void Open()
    {
        _index ??= BuildIndex();

        _matches = null;
        _position = 0;

        var probeValue = _probeEntry.GetValue();
        if (probeValue is not null)
            _index.TryGetValue(probeValue, out _matches);
    }

    private Dictionary<object, List<int>> BuildIndex()
    {
        var index = new Dictionary<object, List<int>>();

        _input.Open();
        while (_input.Read())
        {
            var key = _indexEntry.GetValue();
            if (key is null)
                continue;

            if (!index.TryGetValue(key, out var rows))
            {
                rows = new List<int>();
                index.Add(key, rows);
            }

            _store.Append(_input.RowBuffer);
            rows.Add(_store.Count - 1);
        }

        return index;
    }

    public override void Dispose()
    {
        _input.Dispose();
    }

    public override bool Read()
    {
        if (_matches is null || _position == _matches.Count)
            return false;

        _cursor.Row = _matches[_position];
        _position++;
        return true;
    }
}
