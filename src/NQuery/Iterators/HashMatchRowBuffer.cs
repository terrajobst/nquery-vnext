#nullable enable

namespace NQuery.Iterators;

// The combined build ++ probe output of a hash match. Each side is an indirected
// buffer whose backing can be swapped per row: a build entry's stored row or an
// all-NULL pad (an unmatched outer row), and the live probe row or an all-NULL pad.
internal sealed class HashMatchRowBuffer : RowBuffer
{
    private readonly IndirectedRowBuffer _build;
    private readonly HashMatchEntryRowBuffer _buildEntry;
    private readonly NullRowBuffer _buildNull;

    private readonly IndirectedRowBuffer _probe;
    private readonly NullRowBuffer _probeNull;

    public HashMatchRowBuffer(int buildCount, int probeCount)
    {
        _buildEntry = new HashMatchEntryRowBuffer();
        _buildNull = new NullRowBuffer(buildCount);
        _build = new IndirectedRowBuffer(buildCount, _buildNull);

        _probeNull = new NullRowBuffer(probeCount);
        _probe = new IndirectedRowBuffer(probeCount, _probeNull);
    }

    public void SetBuild(HashMatchEntry? entry)
    {
        _buildEntry.Entry = entry;
        _build.ActiveRowBuffer = entry is null ? _buildNull : _buildEntry;
    }

    public void SetProbe(RowBuffer? rowBuffer)
    {
        _probe.ActiveRowBuffer = rowBuffer ?? _probeNull;
    }

    // The build half on its own -- the output of a semi/anti hash match, which emits
    // build rows only. It tracks the same SetBuild as the combined buffer.
    public RowBuffer Build => _build;

    public override int Count => _buildNull.Count + _probeNull.Count;

    public override object this[int index] =>
        index < _build.Count ? _build[index] : _probe[index - _build.Count];

    public override void CopyTo(object[] array, int destinationIndex)
    {
        _build.CopyTo(array, destinationIndex);
        _probe.CopyTo(array, _build.Count + destinationIndex);
    }
}
