namespace NQuery.CodeAnalysis.Iterators;

// A buffer with no columns, exposed by the empty/constant single-row sources.
internal sealed class EmptyRowBuffer : RowBuffer
{
    public override int ObjectCount => 0;

    public override int Bits32Count => 0;

    public override int Bits64Count => 0;

    public override int Bits128Count => 0;

    public override object? GetObject(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    public override uint GetBits32(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    public override ulong GetBits64(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    public override Bits128 GetBits128(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    public override bool IsNull32(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    public override bool IsNull64(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    public override bool IsNull128(int index) => throw new ArgumentOutOfRangeException(nameof(index));
}
