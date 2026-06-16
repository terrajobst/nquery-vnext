using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.Iterators.RowBuffers;

public class ProbedRowBufferTests : RowBufferTests
{
    private static ArrayRowBuffer Buffer(params object[] values)
    {
        var buffer = new ArrayRowBuffer(values.Length);
        values.CopyTo(buffer.Array, 0);
        return buffer;
    }

    [Fact]
    public void RowBuffers_Probed_AppendsProbeColumn_DefaultsToFalse()
    {
        var buffer = new ProbedRowBuffer(Buffer(1, 2));
        AssertContract(buffer, 1, 2, false);
    }

    [Fact]
    public void RowBuffers_Probed_SetProbe_TogglesTrailingColumn()
    {
        var buffer = new ProbedRowBuffer(Buffer(1, 2));

        buffer.SetProbe(true);
        AssertContract(buffer, 1, 2, true);

        buffer.SetProbe(false);
        AssertContract(buffer, 1, 2, false);
    }

    [Fact]
    public void RowBuffers_Probed_WrapsEmptyInnerBuffer()
    {
        var buffer = new ProbedRowBuffer(Buffer());
        buffer.SetProbe(true);
        AssertContract(buffer, true);
    }

    [Fact]
    public void RowBuffers_Probed_ReflectsUnderlyingWrites()
    {
        var inner = Buffer(1);
        var buffer = new ProbedRowBuffer(inner);
        buffer.SetProbe(true);

        inner.Array[0] = 9;
        AssertContract(buffer, 9, true);
    }
}
