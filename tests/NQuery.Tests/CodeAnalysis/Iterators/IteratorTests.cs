using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

// Mirrors NQuery.Tests.Iterators.IteratorTests, but against the emitted iterator
// contract (NQuery.CodeAnalysis.Iterators).
public class IteratorTests
{
    internal static void AssertEmpty(Iterator iterator)
    {
        iterator.Open();
        Assert.False(iterator.Read());
    }

    internal static void AssertProduces(Iterator iterator, object?[] data)
    {
        var twoDimensional = new object?[data.Length, 1];
        for (var i = 0; i < data.Length; i++)
            twoDimensional[i, 0] = data[i];

        AssertProduces(iterator, twoDimensional);
    }

    internal static void AssertProduces(Iterator iterator, object?[,] data)
    {
        AssertProduces(iterator, RowBufferTestSupport.InferColumnTypes(data), data);
    }

    // Explicit column types -- needed when a column is all-NULL in the data (its type
    // can't be inferred) but the buffer stores it in a bit container.
    internal static void AssertProduces(Iterator iterator, Type[] types, object?[,] data)
    {
        var rowCount = data.GetLength(0);
        var columnCount = data.GetLength(1);

        var layout = RowBufferLayout.Create(types);

        iterator.Open();

        for (var i = 0; i < rowCount; i++)
        {
            Assert.True(iterator.Read());

            var buffer = iterator.RowBuffer;
            Assert.Equal(columnCount, buffer.ObjectCount + buffer.Bits32Count + buffer.Bits64Count + buffer.Bits128Count);

            for (var j = 0; j < columnCount; j++)
                Assert.Equal(data[i, j], buffer.GetBoxedValue(layout.Columns[j], types[j]));
        }

        Assert.False(iterator.Read());
    }
}
