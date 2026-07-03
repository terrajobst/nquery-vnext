namespace NQuery.CodeAnalysis.Iterators;

// A columnar spool for the buffering iterators (Sort, hash-match build): the row set is
// stored unboxed, segregated by container, instead of one ArrayRowBuffer (with its own
// arrays) per row. A row is just an index.
//
// Storage is chunked -- a list of fixed-size blocks, each a small columnar slab for
// ChunkRows rows. Growing the spool appends a new block; it never reallocates the existing
// ones. That matters because the alternative (one flat array per container, doubled via
// Array.Resize) throws away the old array on every growth step, so spooling N rows would
// allocate ~2x the live size in transient array garbage and overshoot capacity by up to 2x.
// Chunks pay neither: total allocation tracks the live size and overshoot is bounded by one
// block. Rows never move once stored, so a sort permutes a cheap int[] of indices instead.
internal sealed class SpooledRowStore
{
    private const int ChunkRows = 256;
    private const int ChunkShift = 8;
    private const int ChunkMask = ChunkRows - 1;

    private readonly int _objectCount;
    private readonly int _bits32Count;
    private readonly int _bits64Count;
    private readonly int _bits128Count;
    private readonly int _null32Words;
    private readonly int _null64Words;
    private readonly int _null128Words;

    private readonly List<Chunk> _chunks = new();
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
    }

    public int Count => _count;

    public int ObjectCount => _objectCount;

    public int Bits32Count => _bits32Count;

    public int Bits64Count => _bits64Count;

    public int Bits128Count => _bits128Count;

    // Copies the source row's containers into the next free slot, raw (no boxing).
    public void Append(RowBuffer source)
    {
        var row = _count;
        var chunkIndex = row >> ChunkShift;
        if (chunkIndex == _chunks.Count)
            _chunks.Add(new Chunk(this));

        var chunk = _chunks[chunkIndex];
        var slot = row & ChunkMask;

        var objectBase = slot * _objectCount;
        for (var i = 0; i < _objectCount; i++)
            chunk.Objects[objectBase + i] = source.GetObject(i);

        var bits32Base = slot * _bits32Count;
        var null32Base = slot * _null32Words;
        for (var i = 0; i < _bits32Count; i++)
        {
            chunk.Bits32[bits32Base + i] = source.GetBits32(i);
            SetNull(chunk.Null32, null32Base, i, source.IsNull32(i));
        }

        var bits64Base = slot * _bits64Count;
        var null64Base = slot * _null64Words;
        for (var i = 0; i < _bits64Count; i++)
        {
            chunk.Bits64[bits64Base + i] = source.GetBits64(i);
            SetNull(chunk.Null64, null64Base, i, source.IsNull64(i));
        }

        var bits128Base = slot * _bits128Count;
        var null128Base = slot * _null128Words;
        for (var i = 0; i < _bits128Count; i++)
        {
            chunk.Bits128[bits128Base + i] = source.GetBits128(i);
            SetNull(chunk.Null128, null128Base, i, source.IsNull128(i));
        }

        _count++;
    }

    // Resets the row count so the chunks (and their capacity) can be reused across
    // re-executions; object references are released for the GC.
    public void Clear()
    {
        if (_objectCount > 0)
        {
            foreach (var chunk in _chunks)
                Array.Clear(chunk.Objects, 0, chunk.Objects.Length);
        }

        _count = 0;
    }

    // A reusable, repositionable RowBuffer view onto a single row. The hash match points
    // its build side at one of these and moves it row to row.
    public Cursor CreateCursor() => new(this);

    public object? GetObject(int row, int index)
    {
        var chunk = _chunks[row >> ChunkShift];
        return chunk.Objects[(row & ChunkMask) * _objectCount + index];
    }

    public uint GetBits32(int row, int index)
    {
        var chunk = _chunks[row >> ChunkShift];
        return chunk.Bits32[(row & ChunkMask) * _bits32Count + index];
    }

    public ulong GetBits64(int row, int index)
    {
        var chunk = _chunks[row >> ChunkShift];
        return chunk.Bits64[(row & ChunkMask) * _bits64Count + index];
    }

    public Int128 GetBits128(int row, int index)
    {
        var chunk = _chunks[row >> ChunkShift];
        return chunk.Bits128[(row & ChunkMask) * _bits128Count + index];
    }

    public bool IsNull32(int row, int index)
    {
        var chunk = _chunks[row >> ChunkShift];
        return GetNull(chunk.Null32, (row & ChunkMask) * _null32Words, index);
    }

    public bool IsNull64(int row, int index)
    {
        var chunk = _chunks[row >> ChunkShift];
        return GetNull(chunk.Null64, (row & ChunkMask) * _null64Words, index);
    }

    public bool IsNull128(int row, int index)
    {
        var chunk = _chunks[row >> ChunkShift];
        return GetNull(chunk.Null128, (row & ChunkMask) * _null128Words, index);
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

    // One fixed-size columnar block: ChunkRows rows' worth of each container. Empty
    // containers share Array.Empty so a narrow row shape costs nothing for the kinds it
    // doesn't use.
    private sealed class Chunk
    {
        public readonly object?[] Objects;
        public readonly uint[] Bits32;
        public readonly ulong[] Bits64;
        public readonly Int128[] Bits128;
        public readonly ulong[] Null32;
        public readonly ulong[] Null64;
        public readonly ulong[] Null128;

        public Chunk(SpooledRowStore store)
        {
            ThrowIfNull(store);

            Objects = store._objectCount == 0 ? [] : new object?[ChunkRows * store._objectCount];
            Bits32 = store._bits32Count == 0 ? [] : new uint[ChunkRows * store._bits32Count];
            Bits64 = store._bits64Count == 0 ? [] : new ulong[ChunkRows * store._bits64Count];
            Bits128 = store._bits128Count == 0 ? [] : new Int128[ChunkRows * store._bits128Count];
            Null32 = store._null32Words == 0 ? [] : new ulong[ChunkRows * store._null32Words];
            Null64 = store._null64Words == 0 ? [] : new ulong[ChunkRows * store._null64Words];
            Null128 = store._null128Words == 0 ? [] : new ulong[ChunkRows * store._null128Words];
        }
    }

    // A RowBuffer view onto one row of the store. Reuses the base class's typed/boxed
    // decode machinery; raw access goes straight to the store's chunks. The position (Row)
    // is mutable so one instance can be walked across rows without allocating.
    internal sealed class Cursor : RowBuffer
    {
        private readonly SpooledRowStore _store;

        public Cursor(SpooledRowStore store)
        {
            ThrowIfNull(store);

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
