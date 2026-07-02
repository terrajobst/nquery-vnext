using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

public class FilterIteratorTests : IteratorTests
{
    [Fact]
    public void Iterators_Filter_ForwardsProperly()
    {
        var rows = new object[] { 1, 2 };
        var expected = rows;

        using var input = new MockedIterator(rows);
        using (var iterator = new FilterIterator(input, _ => true, outer: null))
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
    public void Iterators_Filter_ReturnsEmpty_IfInputIsEmpty()
    {
        var rows = Array.Empty<object>();

        using var input = new MockedIterator(rows);
        using var iterator = new FilterIterator(input, _ => true, outer: null);
        AssertEmpty(iterator);
    }

    [Fact]
    public void Iterators_Filter_ReturnsEmpty_IfPredicateIsFalse()
    {
        using var input = new ConstantIterator();
        using var iterator = new FilterIterator(input, _ => false, outer: null);
        AssertEmpty(iterator);
    }

    [Fact]
    public void Iterators_Filter_Filters()
    {
        var rows = new object[] { 1, 2, 3, 4 };
        var expected = new object[] { 2, 4 };

        using var input = new MockedIterator(rows);
        using var iterator = new FilterIterator(input, rb => rb.NullableInt32(0)!.Value % 2 == 0, outer: null);
        AssertProduces(iterator, expected);
    }

    // Hole: the outer-correlation path. When an outer buffer is
    // supplied, the predicate sees (outer ++ input), so it can reference the outer row.
    [Fact]
    public void Iterators_Filter_EvaluatesPredicateAgainstOuterThenInput()
    {
        var rows = new object[] { 1, 2, 3, 4 };
        var expected = new object[] { 3, 4 };
        var outer = new MockedRowBuffer([2]);

        using var input = new MockedIterator(rows);
        // rb[0] is the outer value (2), rb[1] is the input value -> keep input > outer.
        using var iterator = new FilterIterator(input, rb => rb.NullableInt32(1)!.Value > rb.NullableInt32(0)!.Value, outer);
        AssertProduces(iterator, expected);
    }
}
