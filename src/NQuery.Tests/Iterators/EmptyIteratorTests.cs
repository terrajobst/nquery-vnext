using System;

using NQuery.Iterators;

using Xunit;

namespace NQuery.Tests.Iterators
{
    public class EmptyIteratorTests : IteratorTests
    {
        [Fact]
        public void Iterators_Empty_ReturnsNoRows()
        {
            using (var iterator = new EmptyIterator())
            {
                AssertEmpty(iterator);
            }
        }
    }
}