using System.Collections;

using NQuery.Hosting;
using NQuery.Iterators;
using NQuery.Symbols;

namespace NQuery.Tests.Iterators
{
    public class TableIteratorTests : IteratorTests
    {
        private static TableIterator CreateIterator(int rowCount)
        {
            var rows = Enumerable.Range(0, rowCount);
            return CreateIterator(rows);
        }

        private static TableIterator CreateIterator(IEnumerable rows)
        {
            var table = TableDefinition.Create("Table", rows, typeof(int), NullProviders.PropertyProvider);

            var valueSelectors = new Func<object, object>[]
            {
                r => (int) r*10
            };

            return new TableIterator(table, valueSelectors);
        }

        private sealed class MockedEnumerable : IEnumerable, IEnumerator, IDisposable
        {
            public IEnumerator GetEnumerator()
            {
                return this;
            }

            public void Dispose()
            {
                DisposalCount++;
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }

            public object Current { get; }

            public int DisposalCount { get; private set; }
        }

        private sealed class NonDisposableEnumerable : IEnumerable, IEnumerator
        {
            public IEnumerator GetEnumerator()
            {
                return this;
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }

            public object Current { get; }
        }

        [Fact]
        public void Iterators_Table_ReturnsEmpty_IfInputEmpty()
        {
            using (var iterator = CreateIterator(0))
            {
                AssertEmpty(iterator);
            }
        }

        [Fact]
        public void Iterators_Table_ReturnsRows()
        {
            var expected = new object[] {0, 10, 20};

            using (var iterator = CreateIterator(3))
            {
                AssertProduces(iterator, expected);
            }
        }

        [Fact]
        public void Iterators_Table_DisposesRows()
        {
            var mockedEnumerable = new MockedEnumerable();

            var iterator = CreateIterator(mockedEnumerable);
            iterator.Open();
            iterator.Open();
            iterator.Dispose();

            Assert.Equal(2, mockedEnumerable.DisposalCount);
        }

        [Fact]
        public void Iterators_Table_HandlesNonDisposableRows()
        {
            var mockedEnumerable = new NonDisposableEnumerable();

            using (var iterator = CreateIterator(mockedEnumerable))
            {
                iterator.Open();
                iterator.Open();
            }
        }
    }
}