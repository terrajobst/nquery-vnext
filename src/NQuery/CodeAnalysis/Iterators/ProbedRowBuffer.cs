namespace NQuery.CodeAnalysis.Iterators;

// Wraps a buffer and appends a single boolean "probe" column. A probing semi join uses
// it to report, per outer row, whether a match existed. The boolean is a 32-bit value,
// so it becomes the last column of the 32-bit container.
internal sealed class ProbedRowBuffer : RowBuffer
{
    private readonly RowBuffer _rowBuffer;
    private uint _value;

    public ProbedRowBuffer(RowBuffer rowBuffer)
    {
        _rowBuffer = rowBuffer;
    }

    public void SetProbe(bool value)
    {
        _value = value ? 1u : 0u;
    }

    public override int ObjectCount => _rowBuffer.ObjectCount;

    public override int Bits32Count => _rowBuffer.Bits32Count + 1;

    public override int Bits64Count => _rowBuffer.Bits64Count;

    public override int Bits128Count => _rowBuffer.Bits128Count;

    public override object? GetObject(int index) => _rowBuffer.GetObject(index);

    public override uint GetBits32(int index)
    {
        return index < _rowBuffer.Bits32Count
                   ? _rowBuffer.GetBits32(index)
                   : _value;
    }

    public override ulong GetBits64(int index) => _rowBuffer.GetBits64(index);

    public override Bits128 GetBits128(int index) => _rowBuffer.GetBits128(index);

    public override bool IsNull32(int index)
    {
        return index < _rowBuffer.Bits32Count && _rowBuffer.IsNull32(index);
    }

    public override bool IsNull64(int index) => _rowBuffer.IsNull64(index);

    public override bool IsNull128(int index) => _rowBuffer.IsNull128(index);
}
