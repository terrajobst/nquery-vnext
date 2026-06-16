using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.Iterators.RowBuffers;

public class HashMatchRowBufferTests : RowBufferTests
{
    private static ArrayRowBuffer Buffer(params object[] values)
    {
        var buffer = new ArrayRowBuffer(values.Length);
        values.CopyTo(buffer.Array, 0);
        return buffer;
    }

    [Fact]
    public void RowBuffers_HashMatch_ExposesBuildEntryThenProbe()
    {
        var buffer = new HashMatchRowBuffer(buildCount: 2, probeCount: 1);
        buffer.SetBuild(new HashMatchEntry { RowValues = new object[] { 1, 2 } });
        buffer.SetProbe(Buffer(9));

        AssertContract(buffer, 1, 2, 9);
    }

    [Fact]
    public void RowBuffers_HashMatch_NullBuild_PadsBuildSideWithNull()
    {
        var buffer = new HashMatchRowBuffer(buildCount: 2, probeCount: 1);
        buffer.SetBuild(null);
        buffer.SetProbe(Buffer(9));

        AssertContract(buffer, null, null, 9);
    }

    [Fact]
    public void RowBuffers_HashMatch_NullProbe_PadsProbeSideWithNull()
    {
        var buffer = new HashMatchRowBuffer(buildCount: 2, probeCount: 1);
        buffer.SetBuild(new HashMatchEntry { RowValues = new object[] { 1, 2 } });
        buffer.SetProbe(null);

        AssertContract(buffer, 1, 2, null);
    }

    [Fact]
    public void RowBuffers_HashMatch_ReflectsBuildAndProbeSwaps()
    {
        var buffer = new HashMatchRowBuffer(buildCount: 1, probeCount: 1);

        buffer.SetBuild(new HashMatchEntry { RowValues = new object[] { 1 } });
        buffer.SetProbe(Buffer("a"));
        AssertContract(buffer, 1, "a");

        buffer.SetBuild(new HashMatchEntry { RowValues = new object[] { 2 } });
        buffer.SetProbe(Buffer("b"));
        AssertContract(buffer, 2, "b");
    }
}
