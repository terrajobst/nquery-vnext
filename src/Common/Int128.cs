#if NETFRAMEWORK

// Polyfill for System.Int128, which only exists on .NET 7+. NQuery also targets
// net481, so this provides the type there; the guard keeps it from colliding with
// the framework-provided type on net8.0.
//
// NQuery never treats this as a number: it is used purely as a raw 16-byte storage
// cell for the "> 8 and <= 16 bytes" row-buffer bucket (decimal, Guid). Values are
// reinterpreted into and out of it via Unsafe.As (see ArrayRowBuffer.Read128Bit/
// Write128Bit), so only the 16-byte layout matters, not the numeric API. The two
// fields exist solely to provide that layout -- nothing ever reads them, which is
// exactly why they can be (and are) private, matching the real Int128's shape.
namespace System;

internal readonly struct Int128
{
#pragma warning disable CS0169, CS0649 // backing storage only; reinterpreted via Unsafe.As, never read directly
    private readonly ulong _lower;
    private readonly ulong _upper;
#pragma warning restore CS0169, CS0649
}

#endif
