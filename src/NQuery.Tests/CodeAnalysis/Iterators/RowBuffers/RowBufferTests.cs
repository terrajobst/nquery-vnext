using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators.RowBuffers;

// Shared contract for every RowBuffer: the per-container counts and the typed/boxed
// reads must agree on the values it exposes. A concrete test configures a buffer (and
// any mutator state) then calls AssertContract with the values (and, where NULL hides
// the type, the column types) it should currently expose.
public abstract class RowBufferTests
{
    private protected static void AssertContract(RowBuffer buffer, params object?[] expected)
    {
        var types = expected.Select(v => v?.GetType() ?? typeof(object)).ToArray();
        AssertContract(buffer, expected, types);
    }

    private protected static void AssertContract(RowBuffer buffer, object?[] expected, Type[] types)
    {
        var layout = RowBufferLayout.Create(types);

        var columnCount = buffer.ObjectCount + buffer.Bits32Count + buffer.Bits64Count + buffer.Bits128Count;
        Assert.Equal(expected.Length, columnCount);
        Assert.Equal(layout.ObjectCount, buffer.ObjectCount);
        Assert.Equal(layout.Bits32Count, buffer.Bits32Count);
        Assert.Equal(layout.Bits64Count, buffer.Bits64Count);
        Assert.Equal(layout.Bits128Count, buffer.Bits128Count);

        for (var i = 0; i < expected.Length; i++)
            Assert.Equal(expected[i], buffer.GetBoxedValue(layout.Columns[i], types[i]));
    }

    private protected static ArrayRowBuffer Buffer(params object?[] values)
    {
        return RowBufferTestSupport.Buffer(values);
    }
}
