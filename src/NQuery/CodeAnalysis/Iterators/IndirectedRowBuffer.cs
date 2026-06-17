namespace NQuery.CodeAnalysis.Iterators;

// A buffer whose backing buffer can be swapped at run time -- outer joins flip the
// right side between the real row and an all-NULL row, and concatenation flips between
// inputs. All candidate buffers share one layout, so the container counts are fixed.
internal sealed class IndirectedRowBuffer : RowBuffer
{
    public IndirectedRowBuffer(int objectCount, int bits32Count, int bits64Count, int bits128Count, RowBuffer activeRowBuffer)
    {
        ObjectCount = objectCount;
        Bits32Count = bits32Count;
        Bits64Count = bits64Count;
        Bits128Count = bits128Count;
        ActiveRowBuffer = activeRowBuffer;
    }

    public IndirectedRowBuffer(RowBuffer template, RowBuffer activeRowBuffer)
        : this(template.ObjectCount, template.Bits32Count, template.Bits64Count, template.Bits128Count, activeRowBuffer)
    {
    }

    public RowBuffer ActiveRowBuffer { get; set; }

    public override int ObjectCount { get; }

    public override int Bits32Count { get; }

    public override int Bits64Count { get; }

    public override int Bits128Count { get; }

    public override object? GetObject(int index) => ActiveRowBuffer.GetObject(index);

    public override uint GetBits32(int index) => ActiveRowBuffer.GetBits32(index);

    public override ulong GetBits64(int index) => ActiveRowBuffer.GetBits64(index);

    public override Bits128 GetBits128(int index) => ActiveRowBuffer.GetBits128(index);

    public override bool IsNull32(int index) => ActiveRowBuffer.IsNull32(index);

    public override bool IsNull64(int index) => ActiveRowBuffer.IsNull64(index);

    public override bool IsNull128(int index) => ActiveRowBuffer.IsNull128(index);
}
