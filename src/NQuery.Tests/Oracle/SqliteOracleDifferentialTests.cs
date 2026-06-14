using System.Collections.Generic;

namespace NQuery.Tests.Oracle;

// Differential correctness against SQLite as the oracle: each query runs through the live NQuery
// engine and through SQLite over the same Northwind data, and the result rows must match as a
// multiset (see RowSet). This complements OldEngineDifferentialTests, which compares NQuery to a
// previous build of itself: here the reference is an independent, mature SQL engine, so it can
// catch semantic errors that both NQuery versions share.
//
// When NQuery's spelling differs from SQLite's (e.g. TOP vs LIMIT), pass the SQLite-specific text
// as the second argument; otherwise the same string runs on both engines.
public class SqliteOracleDifferentialTests
{
    [Theory]
    // Scan + filter + project.
    [InlineData("SELECT e.EmployeeID, e.City FROM Employees e WHERE e.Country = 'UK'")]
    [InlineData("SELECT e.EmployeeID, e.EmployeeID + 1 FROM Employees e")]
    // NULL handling and CASE.
    [InlineData("SELECT e.EmployeeID FROM Employees e WHERE e.ReportsTo IS NULL")]
    [InlineData("SELECT e.EmployeeID, CASE WHEN e.City = 'London' THEN 1 ELSE 0 END FROM Employees e")]
    // Sort + row limit (dialect differs: TOP vs LIMIT).
    [InlineData("SELECT TOP 3 e.City FROM Employees e ORDER BY e.City, e.EmployeeID",
                "SELECT e.City FROM Employees e ORDER BY e.City, e.EmployeeID LIMIT 3")]
    // Inner join.
    [InlineData("SELECT o.OrderID, e.City FROM Employees e INNER JOIN Orders o ON e.EmployeeID = o.EmployeeID WHERE o.OrderID < 10260")]
    // Left outer join with NULL padding.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND o.OrderID = 10248")]
    // Correlated EXISTS / NOT EXISTS.
    [InlineData("SELECT e.EmployeeID FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)")]
    [InlineData("SELECT c.CustomerID FROM Customers c WHERE NOT EXISTS (SELECT * FROM Orders o WHERE o.CustomerID = c.CustomerID)")]
    // IN over a subquery.
    [InlineData("SELECT e.EmployeeID, e.ReportsTo FROM Employees e WHERE e.ReportsTo IN (SELECT o.EmployeeID FROM Orders o)")]
    // Correlated scalar subquery (COUNT must be 0, not NULL, for employees with no orders).
    [InlineData("SELECT e.EmployeeID, (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e")]
    // Scalar aggregate over all rows.
    [InlineData("SELECT COUNT(*), MAX(e.EmployeeID), MIN(e.EmployeeID), SUM(e.EmployeeID) FROM Employees e")]
    // Grouped aggregate, including a NULL-bearing grouping column and a multi-column grouping.
    [InlineData("SELECT e.City, COUNT(*) FROM Employees e GROUP BY e.City")]
    [InlineData("SELECT e.ReportsTo, COUNT(*) FROM Employees e GROUP BY e.ReportsTo")]
    [InlineData("SELECT e.Country, e.City, COUNT(*) FROM Employees e GROUP BY e.Country, e.City")]
    // Set operators.
    [InlineData("SELECT e.City FROM Employees e UNION ALL SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e UNION SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e INTERSECT SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e EXCEPT SELECT c.City FROM Customers c")]
    public void NQuery_ProducesSameRows_AsSqlite(string nqueryText, string sqliteText = null)
    {
        var expected = SqliteOracle.RunQuery(sqliteText ?? nqueryText);
        var actual = RunNQuery(nqueryText);

        RowSet.AssertEqualUnordered(expected, actual);
    }

    [Theory]
    // Order is fully determined by an ORDER BY on a non-null integer key, so the two engines must
    // return the rows in the same sequence.
    [InlineData("SELECT e.EmployeeID, e.City FROM Employees e ORDER BY e.EmployeeID")]
    // Sort + row limit (dialect differs: TOP vs LIMIT); the limit makes order observable.
    [InlineData("SELECT TOP 3 e.EmployeeID FROM Employees e ORDER BY e.EmployeeID",
                "SELECT e.EmployeeID FROM Employees e ORDER BY e.EmployeeID LIMIT 3")]
    public void NQuery_ProducesRowsInSameOrder_AsSqlite(string nqueryText, string sqliteText = null)
    {
        var expected = SqliteOracle.RunQuery(sqliteText ?? nqueryText);
        var actual = RunNQuery(nqueryText);

        RowSet.AssertEqualOrdered(expected, actual);
    }

    private static List<object[]> RunNQuery(string text)
    {
        using var reader = Query.Create(NorthwindDataContext.Instance, text).ExecuteReader();

        var rows = new List<object[]>();
        while (reader.Read())
        {
            var row = new object[reader.ColumnCount];
            for (var i = 0; i < row.Length; i++)
                row[i] = RowSet.Normalize(reader[i]);
            rows.Add(row);
        }

        return rows;
    }
}
