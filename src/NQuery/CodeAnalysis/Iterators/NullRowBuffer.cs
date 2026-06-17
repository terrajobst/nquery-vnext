namespace NQuery.CodeAnalysis.Iterators;

// A fixed-width buffer that is all-NULL in every container -- the right side of an
// unmatched outer-join row. Sized to mirror the side it stands in for.
internal sealed class NullRowBuffer : RowBuffer
{
    public NullRowBuffer(int objectCount, int bits32Count, int bits64Count, int bits128Count)
    {
        ObjectCount = objectCount;
        Bits32Count = bits32Count;
        Bits64Count = bits64Count;
        Bits128Count = bits128Count;
    }

    public NullRowBuffer(RowBuffer template)
        : this(template.ObjectCount, template.Bits32Count, template.Bits64Count, template.Bits128Count)
    {
    }

    public override int ObjectCount { get; }

    public override int Bits32Count { get; }

    public override int Bits64Count { get; }

    public override int Bits128Count { get; }

    public override object? GetObject(int index) => null;

    public override uint GetBits32(int index) => 0;

    public override ulong GetBits64(int index) => 0;

    public override Int128 GetBits128(int index) => default;

    public override bool IsNull32(int index) => true;

    public override bool IsNull64(int index) => true;

    public override bool IsNull128(int index) => true;
}
