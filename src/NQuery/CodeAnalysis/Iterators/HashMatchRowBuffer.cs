namespace NQuery.CodeAnalysis.Iterators;

// The combined build ++ probe output of a hash match. Each side is an indirected buffer
// whose backing can be swapped per row: a build entry's materialized row or an all-NULL
// pad (an unmatched outer row), and the live probe row or an all-NULL pad. The combined
// view glues the two sides per container (via CombinedRowBuffer).
internal sealed class HashMatchRowBuffer : RowBuffer
{
    private readonly IndirectedRowBuffer _build;
    private readonly NullRowBuffer _buildNull;

    private readonly IndirectedRowBuffer _probe;
    private readonly NullRowBuffer _probeNull;

    private readonly CombinedRowBuffer _combined;

    public HashMatchRowBuffer(RowBuffer buildTemplate, RowBuffer probeTemplate)
    {
        _buildNull = new NullRowBuffer(buildTemplate);
        _build = new IndirectedRowBuffer(buildTemplate, _buildNull);

        _probeNull = new NullRowBuffer(probeTemplate);
        _probe = new IndirectedRowBuffer(probeTemplate, _probeNull);

        _combined = new CombinedRowBuffer(_build, _probe);
    }

    public void SetBuild(HashMatchEntry? entry)
    {
        _build.ActiveRowBuffer = entry is null ? _buildNull : entry.Row;
    }

    public void SetProbe(RowBuffer? rowBuffer)
    {
        _probe.ActiveRowBuffer = rowBuffer ?? _probeNull;
    }

    // The build half on its own -- the output of a semi/anti hash match, which emits
    // build rows only. It tracks the same SetBuild as the combined buffer.
    public RowBuffer Build => _build;

    public override int ObjectCount => _combined.ObjectCount;

    public override int Bits32Count => _combined.Bits32Count;

    public override int Bits64Count => _combined.Bits64Count;

    public override int Bits128Count => _combined.Bits128Count;

    public override object? GetObject(int index) => _combined.GetObject(index);

    public override uint GetBits32(int index) => _combined.GetBits32(index);

    public override ulong GetBits64(int index) => _combined.GetBits64(index);

    public override Bits128 GetBits128(int index) => _combined.GetBits128(index);

    public override bool IsNull32(int index) => _combined.IsNull32(index);

    public override bool IsNull64(int index) => _combined.IsNull64(index);

    public override bool IsNull128(int index) => _combined.IsNull128(index);
}
