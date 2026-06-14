using NQuery.Iterators;

namespace NQuery.Tests.Iterators;

public class EmittedAssertIteratorTests : IteratorTests
{
    [Fact]
    public void Iterators_EmittedAssert_ForwardsProperly()
    {
        var rows = new object[] { 1, 2 };
        var expected = rows;

        using var input = new MockedIterator(rows);
        using (var iterator = new EmittedAssertIterator(input, _ => true, string.Empty, outer: null))
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
    public void Iterators_EmittedAssert_DoesNotTriggerAssert_WhenTrue()
    {
        var rows = new object[] { 1 };
        var expected = rows;

        using var input = new MockedIterator(rows);
        using var iterator = new EmittedAssertIterator(input, _ => true, string.Empty, outer: null);

        AssertProduces(iterator, expected);
    }

    [Fact]
    public void Iterators_EmittedAssert_DoesNotTriggerAssert_WhenFalseAndInputEmpty()
    {
        var rows = Array.Empty<object>();
        var expected = rows;

        using var input = new MockedIterator(rows);
        using var iterator = new EmittedAssertIterator(input, _ => false, string.Empty, outer: null);

        AssertProduces(iterator, expected);
    }

    [Fact]
    public void Iterators_EmittedAssert_TriggersAssert_WhenFalseAndInputNonEmpty()
    {
        const string message = "This should be detected";

        var rows = new object[] { 1 };

        using var input = new MockedIterator(rows);
        using var iterator = new EmittedAssertIterator(input, _ => false, message, outer: null);
        iterator.Open();

        var exception = Assert.Throws<InvalidOperationException>(() => iterator.Read());
        Assert.Equal(message, exception.Message);
    }

    // Hole: the outer-correlation path. The predicate sees (outer ++ input).
    [Fact]
    public void Iterators_EmittedAssert_EvaluatesPredicateAgainstOuterThenInput()
    {
        var rows = new object[] { 5 };
        var outer = new MockedRowBuffer(new object[] { 5 });

        using var input = new MockedIterator(rows);
        // Asserts outer == input; both are 5, so it passes.
        using var iterator = new EmittedAssertIterator(input, rb => Equals(rb[0], rb[1]), "mismatch", outer);
        AssertProduces(iterator, rows);
    }
}
