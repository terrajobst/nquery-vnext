using System.Collections.Immutable;

using NQuery.Northwind;

namespace NQuery.Dynamic.Tests;

public class DynamicTests
{
    [Fact]
    public void Dynamic_ExecuteDynamicSequenceAllowsLateBoundAccess()
    {
        var text = """
            SELECT  c.CategoryID,
                    c.CategoryName
            FROM    Categories c
            WHERE   c.CategoryID < 3
            ORDER   BY 1
            """;

        var catalog = NorthwindCatalog.Instance;
        var query = Query.Create(catalog, text);

        var rows = query.ExecuteDynamicSequence().ToImmutableArray();

        Assert.Equal(1, rows[0].CategoryID);
        Assert.Equal("Beverages", rows[0].CategoryName);

        Assert.Equal(2, rows[1].CategoryID);
        Assert.Equal("Condiments", rows[1].CategoryName);
    }
}
