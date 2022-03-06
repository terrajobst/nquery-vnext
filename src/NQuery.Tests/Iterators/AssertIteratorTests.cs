using NQuery.Iterators;

namespace NQuery.Tests.Iterators
{
    public class AssertIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_Assert_ForwardsProperly()
        {
            var rows = new object[] { 1, 2 };
            var expected = rows;

            using var input = new MockedIterator(rows);
            using (var iterator = new AssertIterator(input, () => true, string.Empty))
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
        public void Iterators_Assert_DoesNotTriggerAssert_WhenTrue()
        {
            var rows = new object[] { 1 };
            var expected = rows;

            using var input = new MockedIterator(rows);
            using var iterator = new AssertIterator(input, () => true, string.Empty);

            AssertProduces(iterator, expected);
        }

        [Fact]
        public void Iterators_Assert_DoesNotTriggerAssert_WhenFalseAndInputEmpty()
        {
            var rows = Array.Empty<object>();
            var expected = rows;

            using var input = new MockedIterator(rows);
            using var iterator = new AssertIterator(input, () => false, string.Empty);

            AssertProduces(iterator, expected);
        }

        [Fact]
        public void Iterators_Assert_TriggersAssert_WhenFalseAndInputNonEmpty()
        {
            const string message = "This should be detected";

            var rows = new object[] { 1 };

            using var input = new MockedIterator(rows);
            using var iterator = new AssertIterator(input, () => false, message);
            iterator.Open();

            var exception = Assert.Throws<InvalidOperationException>(() => iterator.Read());
            Assert.Equal(exception.Message, message);
        }
    }
}