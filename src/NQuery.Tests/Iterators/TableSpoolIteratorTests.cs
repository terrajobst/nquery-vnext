using NQuery.Iterators;

using Xunit;

namespace NQuery.Tests.Iterators
{
    public class TableSpoolIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_TableSpool_ForwardsProperly()
        {
            var rows = new object[] { 1, 2 };
            var expected = rows;

            using (var input = new MockedIterator(rows))
            {
                var tableSpoolStack = new TableSpoolStack(1);

                using (var iterator = new TableSpoolIterator(input, tableSpoolStack))
                {
                    for (var i = 0; i < 2; i++)
                    {
                        AssertProduces(iterator, expected);
                    }
                }

                Assert.Equal(2, input.TotalOpenCount);
                Assert.Equal(4, input.TotalReadCount);
                Assert.Equal(1, input.DisposalCount);
            }
        }

        [Fact]
        public void Iterators_TableSpool_ReturnsEmpty_IfInputEmpty()
        {
            var rows = new object[0];
            var tableSpoolStack = new TableSpoolStack(1);

            using (var input = new MockedIterator(rows))
            using (var iterator = new TableSpoolIterator(input, tableSpoolStack))
            {
                AssertEmpty(iterator);
                Assert.True(tableSpoolStack.IsEmpty);
            }
        }

        [Fact]
        public void Iterators_TableSpool_ReturnsInput_AndPushes()
        {
            var rows = new object[,]
            {
                {1, "One"},
                {2, "Two"}
            };

            var expected = rows;

            var tableSpoolStack = new TableSpoolStack(1);
            using (var input = new MockedIterator(rows))
            using (var iterator = new TableSpoolIterator(input, tableSpoolStack))
            {
                AssertProduces(iterator, expected);

                var stackRows = new List<RowBuffer>();
                while (!tableSpoolStack.IsEmpty)
                    stackRows.Add(tableSpoolStack.Pop());

                for (var i = 0; i < rows.GetLength(0); i++)
                {
                    var originalLine = rows.GetLength(0) - i - 1;
                    for (var j = 0; j < rows.GetLength(1); j++)
                        Assert.Equal(rows[originalLine, j], stackRows[i][j]);
                }
            }
        }
    }
}