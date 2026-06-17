using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators.RowBuffers;

public class CombinedRowBufferTests : RowBufferTests
{
    [Fact]
    public void RowBuffers_Combined_ConcatenatesAcrossTheBoundary()
    {
        var combined = new CombinedRowBuffer(Buffer(1, 2), Buffer("a"));
        AssertContract(combined, 1, 2, "a");
    }

    [Fact]
    public void RowBuffers_Combined_HandlesEmptyLeft()
    {
        var combined = new CombinedRowBuffer(Buffer(), Buffer(1, 2));
        AssertContract(combined, 1, 2);
    }

    [Fact]
    public void RowBuffers_Combined_HandlesEmptyRight()
    {
        var combined = new CombinedRowBuffer(Buffer(1, 2), Buffer());
        AssertContract(combined, 1, 2);
    }

    [Fact]
    public void RowBuffers_Combined_GluesSameContainerPerSide()
    {
        // Both sides contribute 32-bit columns, so the right side's column sits after the
        // left's within the 32-bit container.
        var combined = new CombinedRowBuffer(Buffer(1), Buffer(2));
        AssertContract(combined, 1, 2);
        Assert.Equal(2, combined.Bits32Count);
    }

    [Fact]
    public void RowBuffers_Combined_ReflectsUnderlyingWrites()
    {
        var left = Buffer(1);
        var right = Buffer(2);
        var combined = new CombinedRowBuffer(left, right);

        left.Write32Bit<int>(0, 10);
        right.Write32Bit<int>(0, 20);

        AssertContract(combined, 10, 20);
    }
}
