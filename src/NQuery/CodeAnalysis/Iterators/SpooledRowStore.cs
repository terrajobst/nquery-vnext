namespace NQuery.CodeAnalysis.Iterators;

// A columnar spool for the buffering iterators (Sort): instead of one ArrayRowBuffer per
// row -- each owning up to four backing arrays plus null masks -- the whole row set shares
// one set of arrays per container, laid out row-major. A row is just an index; column c of
// row r lives at r * <count> + c. Appending N rows therefore allocates O(containers) arrays
// total (amortized by doubling) rather than O(N * containers), which is what made the
// per-row Clone() path allocate -- and collect -- so heavily.
//
// Rows are never moved once stored; callers sort an int[] of row indices instead (see
// SortIterator), so a sort only permutes a cheap index array, not the row data.
internal sealed class SpooledRowStore
{
    private readonly int _objectCount;
    private readonly int _bits32Count;
    private readonly int _bits64Count;
    private readonly int _bits128Count;
    private readonly int _null32Words;
    private readonly int _null64Words;
    private readonly int _null128Words;

    private object?[] _objects;
    private uint[] _bits32;
    private ulong[] _bits64;
    private Int128[] _bits128;
    private ulong[] _null32;
    private ulong[] _null64;
    private ulong[] _null128;

    private int _capacity;
    private int _count;

    public SpooledRowStore(RowBuffer template)
        : this(template.ObjectCount, template.Bits32Count, template.Bits64Count, template.Bits128Count)
    {
    }

    public SpooledRowStore(int objectCount, int bits32Count, int bits64Count, int bits128Count)
    {
        _objectCount = objectCount;
        _bits32Count = bits32Count;
        _bits64Count = bits64Count;
        _bits128Count = bits128Count;
        _null32Words = WordCount(bits32Count);
        _null64Words = WordCount(bits64Count);
        _null128Words = WordCount(bits128Count);

        _capacity = 0;
        _objects = Array.Empty<object?>();
        _bits32 = Array.Empty<uint>();
        _bits64 = Array.Empty<ulong>();
        _bits128 = Array.Empty<Int128>();
        _null32 = Array.Empty<ulong>();
        _null64 = Array.Empty<ulong>();
        _null128 = Array.Empty<ulong>();
    }

    public int Count => _count;

    public int ObjectCount => _objectCount;

    public int Bits32Count => _bits32Count;

    public int Bits64Count => _bits64Count;

    public int Bits128Count => _bits128Count;

    // Copies the source row's containers into the next free slot, raw (no boxing), exactly
    // as RowBuffer.CopyContainersInto would into a standalone ArrayRowBuffer.
    public void Append(RowBuffer source)
    {
        EnsureCapacity(_count + 1);
        var row = _count;

        var objectBase = row * _objectCount;
        for (var i = 0; i < _objectCount; i++)
            _objects[objectBase + i] = source.GetObject(i);

        var bits32Base = row * _bits32Count;
        var null32Base = row * _null32Words;
        for (var i = 0; i < _bits32Count; i++)
        {
            _bits32[bits32Base + i] = source.GetBits32(i);
            SetNull(_null32, null32Base, i, source.IsNull32(i));
        }

        var bits64Base = row * _bits64Count;
        var null64Base = row * _null64Words;
        for (var i = 0; i < _bits64Count; i++)
        {
            _bits64[bits64Base + i] = source.GetBits64(i);
            SetNull(_null64, null64Base, i, source.IsNull64(i));
        }

        var bits128Base = row * _bits128Count;
        var null128Base = row * _null128Words;
        for (var i = 0; i < _bits128Count; i++)
        {
            _bits128[bits128Base + i] = source.GetBits128(i);
            SetNull(_null128, null128Base, i, source.IsNull128(i));
        }

        _count++;
    }

    // Resets the row count so the storage (and its capacity) can be reused across
    // re-executions; object references in the used range are released for the GC.
    public void Clear()
    {
        if (_objectCount > 0)
            Array.Clear(_objects, 0, _count * _objectCount);

        _count = 0;
    }

    // A reusable, repositionable RowBuffer view onto a single row. The hash match points
    // its build side at one of these and moves it row to row.
    public Cursor CreateCursor() => new(this);

    public object? GetObject(int row, int index) => _objects[row * _objectCount + index];

    public uint GetBits32(int row, int index) => _bits32[row * _bits32Count + index];

    public ulong GetBits64(int row, int index) => _bits64[row * _bits64Count + index];

    public Int128 GetBits128(int row, int index) => _bits128[row * _bits128Count + index];

    public bool IsNull32(int row, int index) => GetNull(_null32, row * _null32Words, index);

    public bool IsNull64(int row, int index) => GetNull(_null64, row * _null64Words, index);

    public bool IsNull128(int row, int index) => GetNull(_null128, row * _null128Words, index);

    private void EnsureCapacity(int required)
    {
        if (required <= _capacity)
            return;

        var newCapacity = _capacity == 0 ? 16 : _capacity * 2;
        if (newCapacity < required)
            newCapacity = required;

        if (_objectCount > 0) Array.Resize(ref _objects, newCapacity * _objectCount);
        if (_bits32Count > 0) Array.Resize(ref _bits32, newCapacity * _bits32Count);
        if (_bits64Count > 0) Array.Resize(ref _bits64, newCapacity * _bits64Count);
        if (_bits128Count > 0) Array.Resize(ref _bits128, newCapacity * _bits128Count);
        if (_null32Words > 0) Array.Resize(ref _null32, newCapacity * _null32Words);
        if (_null64Words > 0) Array.Resize(ref _null64, newCapacity * _null64Words);
        if (_null128Words > 0) Array.Resize(ref _null128, newCapacity * _null128Words);

        _capacity = newCapacity;
    }

    private static int WordCount(int count) => (count + 63) >> 6;

    private static bool GetNull(ulong[] mask, int wordBase, int index)
    {
        return (mask[wordBase + (index >> 6)] & (1UL << (index & 63))) != 0;
    }

    private static void SetNull(ulong[] mask, int wordBase, int index, bool value)
    {
        var word = wordBase + (index >> 6);
        var bit = 1UL << (index & 63);
        if (value)
            mask[word] |= bit;
        else
            mask[word] &= ~bit;
    }

    // A RowBuffer view onto one row of the store. Reuses the base class's typed/boxed
    // decode machinery; raw access goes straight to the store's arrays. The position
    // (Row) is mutable so one instance can be walked across rows without allocating.
    internal sealed class Cursor : RowBuffer
    {
        private readonly SpooledRowStore _store;

        public Cursor(SpooledRowStore store)
        {
            _store = store;
        }

        public int Row { get; set; }

        public override int ObjectCount => _store._objectCount;

        public override int Bits32Count => _store._bits32Count;

        public override int Bits64Count => _store._bits64Count;

        public override int Bits128Count => _store._bits128Count;

        public override object? GetObject(int index) => _store.GetObject(Row, index);

        public override uint GetBits32(int index) => _store.GetBits32(Row, index);

        public override ulong GetBits64(int index) => _store.GetBits64(Row, index);

        public override Int128 GetBits128(int index) => _store.GetBits128(Row, index);

        public override bool IsNull32(int index) => _store.IsNull32(Row, index);

        public override bool IsNull64(int index) => _store.IsNull64(Row, index);

        public override bool IsNull128(int index) => _store.IsNull128(Row, index);
    }
}
