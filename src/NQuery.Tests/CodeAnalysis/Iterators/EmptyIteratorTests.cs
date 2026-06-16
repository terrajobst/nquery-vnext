using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

public class EmptyIteratorTests : IteratorTests
{
    [Fact]
    public void Iterators_Empty_ReturnsNoRows()
    {
        using var iterator = new EmptyIterator();
        AssertEmpty(iterator);
    }
}
