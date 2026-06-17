namespace NQuery.CodeAnalysis.Iterators;

// Raw 16-byte storage cell for the "> 8 and <= 16 bytes" bucket (decimal, Guid). The
// two ulongs are never interpreted as numbers; they are just the backing bits a value
// is reinterpreted into and out of (see ArrayRowBuffer.Read128Bit/Write128Bit).
internal struct Bits128
{
    public ulong Lo;
    public ulong Hi;
}
