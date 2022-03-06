using System.Collections;

using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class DistinctSortIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_DistinctSort_ForwardsProperly()
        {
            var rows = new object[] { 1, 2 };
            var expected = rows;

            using var input = new MockedIterator(rows);
            var sortEntries = new[] { new RowBufferEntry(input.RowBuffer, 0) };
            var comparers = new[] { Comparer.Default };

            using (var iterator = new DistinctSortIterator(input, sortEntries, comparers))
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

        [Fact]
        public void Iterators_DistinctSort_ReturnsEmpty_IfInputIsEmpty()
        {
            var rows = Array.Empty<object>();

            using var input = new MockedIterator(rows);
            var sortEntries = new[] { new RowBufferEntry(input.RowBuffer, 0) };
            var comparers = new[] { Comparer.Default };

            using var iterator = new DistinctSortIterator(input, sortEntries, comparers);
            AssertEmpty(iterator);
        }

        [Fact]
        public void Iterators_DistinctSort_Sorts_SingleEntry()
        {
            var rows = new object[] { null, 1, 2, 1 };
            var expected = new object[] { null, 2, 1 };

            using var input = new MockedIterator(rows);
            var sortEntries = new[]
            {
                new RowBufferEntry(input.RowBuffer, 0)
            };

            var comparers = new[]
            {
                Comparer<int>.Create((x, y) => y.CompareTo(x))
            };

            using var iterator = new DistinctSortIterator(input, sortEntries, comparers);
            AssertProduces(iterator, expected);
        }

        [Fact]
        public void Iterators_DistinctSort_Sorts_TwoEntries()
        {
            var rows = new object[,]
            {
                {"Kirkland", "USA"},
                {"London", "UK"},
                {"London", null},
                {"Redmond", "USA"},
                {"Seattle", "USA"},
                {"London", "UK"},
                {"Seattle", "USA"},
                {"Tacoma", "USA"},
                {"London", "UK"},
                {null, "USA"},
                {null, null}
            };

            var expected = new object[,]
            {
                {null, null},
                {"London", null},
                {null, "USA"},
                {"Kirkland", "USA"},
                {"Redmond", "USA"},
                {"Seattle", "USA"},
                {"Tacoma", "USA"},
                {"London", "UK"}
            };

            using var input = new MockedIterator(rows);
            var sortEntries = new[]
            {
                new RowBufferEntry(input.RowBuffer, 1),
                new RowBufferEntry(input.RowBuffer, 0)
            };

            var comparers = new[]
            {
                Comparer<string>.Create((x, y) => y.CompareTo(x)),
                Comparer<string>.Create((x, y) => x.CompareTo(y))
            };

            using var iterator = new DistinctSortIterator(input, sortEntries, comparers);
            AssertProduces(iterator, expected);
        }
    }
}