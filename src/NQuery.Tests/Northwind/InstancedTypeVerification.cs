using NQuery.Northwind;

namespace NQuery.Tests.Northwind;

public class InstancedTypeVerification
{
    [Theory]
    [InlineData("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryID")]
    [InlineData("SELECT CustomerID, Region, PostalCode, Fax FROM Customers ORDER BY CustomerID")]
    [InlineData("SELECT EmployeeID, ReportsTo, Region FROM Employees ORDER BY EmployeeID")]
    [InlineData("SELECT OrderID, ShippedDate, ShipRegion, ShipPostalCode FROM Orders ORDER BY OrderID")]
    [InlineData("SELECT OrderID, ProductID, UnitPrice, Quantity, Discount FROM [Order Details] ORDER BY OrderID, ProductID")]
    [InlineData("SELECT od.ProductID, COUNT(*), SUM(od.UnitPrice * od.Quantity) FROM [Order Details] od GROUP BY od.ProductID ORDER BY od.ProductID")]
    [InlineData("SELECT c.Country, COUNT(*) FROM Customers c INNER JOIN Orders o ON c.CustomerID = o.CustomerID GROUP BY c.Country ORDER BY c.Country")]
    [InlineData("SELECT e1.EmployeeID, e2.LastName FROM Employees e1 LEFT JOIN Employees e2 ON e1.ReportsTo = e2.EmployeeID ORDER BY e1.EmployeeID")]
    public void InstancedType_MatchesInstance(string sql)
    {
        var expected = Run(NorthwindCatalog.Instance, sql);
        var actual = Run(NorthwindCatalog.InstanceTyped, sql);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InstancedType_HasSameTablesAndRelationships()
    {
        var instance = NorthwindCatalog.Instance;
        var typed = NorthwindCatalog.InstanceTyped;

        Assert.Equal(
            instance.Tables.Select(t => t.Name).OrderBy(n => n),
            typed.Tables.Select(t => t.Name).OrderBy(n => n));

        Assert.Equal(instance.Relationships.Length, typed.Relationships.Length);

        // Every typed table is backed by a real CLR record, not DataRow.
        Assert.All(typed.Tables, t => Assert.NotEqual(typeof(System.Data.DataRow), t.RowType));
    }

    private static List<string> Run(Catalog catalog, string sql)
    {
        var query = Query.Create(catalog, sql);
        using var reader = query.ExecuteReader();

        var rows = new List<string>();
        while (reader.Read())
        {
            var cells = new string[reader.ColumnCount];
            for (var i = 0; i < reader.ColumnCount; i++)
                cells[i] = reader[i]?.ToString() ?? "<null>";
            rows.Add(string.Join("|", cells));
        }

        return rows;
    }
}
