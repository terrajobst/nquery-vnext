using System;
using System.Collections.Immutable;

using Xunit;

namespace NQuery.Dynamic.Tests
{
    public class DynamicTests
    {
        [Fact]
        public void Dynamic_ExecuteDynamicSequenceAllowsLateBoundAccess()
        {
            var text = @"
                SELECT  c.CategoryID,
                        c.CategoryName
                FROM    Categories c
                WHERE   c.CategoryID < 3
                ORDER   BY 1
            ";

            var dataContext = NorthwindDataContext.Instance;
            var query = new Query(dataContext, text);

            var rows = query.ExecuteDynamicSequence().ToImmutableArray();

            Assert.Equal(1, rows[0].CategoryID);
            Assert.Equal("Beverages", rows[0].CategoryName);

            Assert.Equal(2, rows[1].CategoryID);
            Assert.Equal("Condiments", rows[1].CategoryName);
        }
    }
}