using NQuery.Iterators;

namespace NQuery.Tests.Iterators.RowBuffers;

public class ProjectedRowBufferTests : RowBufferTests
{
    private static ArrayRowBuffer Buffer(params object[] values)
    {
        var buffer = new ArrayRowBuffer(values.Length);
        values.CopyTo(buffer.Array, 0);
        return buffer;
    }

    [Fact]
    public void RowBuffers_Projected_Empty()
    {
        var buffer = new ProjectedRowBuffer(Enumerable.Empty<RowBufferEntry>());
        AssertContract(buffer);
    }

    [Fact]
    public void RowBuffers_Projected_ReordersColumns()
    {
        var source = Buffer(1, "Two");
        var entries = new[]
        {
            new RowBufferEntry(source, 1),
            new RowBufferEntry(source, 0)
        };

        var buffer = new ProjectedRowBuffer(entries);
        AssertContract(buffer, "Two", 1);
    }

    [Fact]
    public void RowBuffers_Projected_SelectsSubset()
    {
        var source = Buffer(1, 2, 3);
        var entries = new[] { new RowBufferEntry(source, 1) };

        var buffer = new ProjectedRowBuffer(entries);
        AssertContract(buffer, 2);
    }

    [Fact]
    public void RowBuffers_Projected_ProjectsAcrossMultipleSources()
    {
        var left = Buffer("L0", "L1");
        var right = Buffer("R0");
        var entries = new[]
        {
            new RowBufferEntry(right, 0),
            new RowBufferEntry(left, 1)
        };

        var buffer = new ProjectedRowBuffer(entries);
        AssertContract(buffer, "R0", "L1");
    }

    [Fact]
    public void RowBuffers_Projected_ReflectsUnderlyingWrites()
    {
        var source = Buffer(1);
        var buffer = new ProjectedRowBuffer(new[] { new RowBufferEntry(source, 0) });

        source.Array[0] = 42;
        AssertContract(buffer, 42);
    }
}
