using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class IteratorTests
    {
        internal static void AssertEmpty(Iterator iterator)
        {
            iterator.Open();
            Assert.False(iterator.Read());
        }

        internal static void AssertProduces(Iterator iterator, object[] data)
        {
            var twoDimensional = new object[data.Length, 1];
            for (var i = 0; i < data.Length; i++)
                twoDimensional[i, 0] = data[i];

            AssertProduces(iterator, twoDimensional);
        }

        internal static void AssertProduces(Iterator iterator, object[,] data)
        {
            var rowCount = data.GetLength(0);
            var entryCount = data.GetLength(1);

            iterator.Open();

            for (var i = 0; i < rowCount; i++)
            {
                Assert.True(iterator.Read());

                Assert.Equal(entryCount, iterator.RowBuffer.Count);

                var row = new object[iterator.RowBuffer.Count];
                iterator.RowBuffer.CopyTo(row, 0);

                for (var j = 0; j < entryCount; j++)
                {
                    Assert.Equal(data[i, j], iterator.RowBuffer[j]);
                    Assert.Equal(data[i, j], row[j]);
                }
            }

            Assert.False(iterator.Read());
        }
    }
}