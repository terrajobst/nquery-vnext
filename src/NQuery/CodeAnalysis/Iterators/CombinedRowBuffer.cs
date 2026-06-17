namespace NQuery.CodeAnalysis.Iterators;

// Concatenates two buffers. Because columns keep their container across the join, the
// gluing is per container: the right side's columns sit after the left's within each
// of the four containers, so a right column's index is left.<Container>Count + offset.
internal sealed class CombinedRowBuffer : RowBuffer
{
    private readonly RowBuffer _left;
    private readonly RowBuffer _right;

    public CombinedRowBuffer(RowBuffer left, RowBuffer right)
    {
        _left = left;
        _right = right;
    }

    public override int ObjectCount => _left.ObjectCount + _right.ObjectCount;

    public override int Bits32Count => _left.Bits32Count + _right.Bits32Count;

    public override int Bits64Count => _left.Bits64Count + _right.Bits64Count;

    public override int Bits128Count => _left.Bits128Count + _right.Bits128Count;

    public override object? GetObject(int index)
    {
        return index < _left.ObjectCount
                   ? _left.GetObject(index)
                   : _right.GetObject(index - _left.ObjectCount);
    }

    public override uint GetBits32(int index)
    {
        return index < _left.Bits32Count
                   ? _left.GetBits32(index)
                   : _right.GetBits32(index - _left.Bits32Count);
    }

    public override ulong GetBits64(int index)
    {
        return index < _left.Bits64Count
                   ? _left.GetBits64(index)
                   : _right.GetBits64(index - _left.Bits64Count);
    }

    public override Int128 GetBits128(int index)
    {
        return index < _left.Bits128Count
                   ? _left.GetBits128(index)
                   : _right.GetBits128(index - _left.Bits128Count);
    }

    public override bool IsNull32(int index)
    {
        return index < _left.Bits32Count
                   ? _left.IsNull32(index)
                   : _right.IsNull32(index - _left.Bits32Count);
    }

    public override bool IsNull64(int index)
    {
        return index < _left.Bits64Count
                   ? _left.IsNull64(index)
                   : _right.IsNull64(index - _left.Bits64Count);
    }

    public override bool IsNull128(int index)
    {
        return index < _left.Bits128Count
                   ? _left.IsNull128(index)
                   : _right.IsNull128(index - _left.Bits128Count);
    }
}
