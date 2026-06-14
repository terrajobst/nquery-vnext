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
    // CROSS APPLY, uncorrelated: the right is independent of the left, so it is the same as a
    // CROSS JOIN to the derived table (which SQLite supports).
    [InlineData("SELECT e.EmployeeID, x.OrderCount FROM Employees e CROSS APPLY (SELECT COUNT(*) AS OrderCount FROM Orders o) x",
                "SELECT e.EmployeeID, x.OrderCount FROM Employees e CROSS JOIN (SELECT COUNT(*) AS OrderCount FROM Orders o) x")]
    // CROSS APPLY, correlated to the left: equivalent to an INNER JOIN on the correlation
    // predicate (SQLite has no APPLY/LATERAL, so the join form is the oracle).
    [InlineData("SELECT e.EmployeeID, oa.OrderID FROM Employees e CROSS APPLY (SELECT o.OrderID FROM Orders o WHERE o.EmployeeID = e.EmployeeID) oa WHERE oa.OrderID < 10260",
                "SELECT e.EmployeeID, o.OrderID FROM Employees e INNER JOIN Orders o ON o.EmployeeID = e.EmployeeID WHERE o.OrderID < 10260")]
    // CROSS APPLY, correlated: a left row with no matching right rows is dropped (inner
    // semantics) -- customers with no qualifying order do not appear.
    [InlineData("SELECT c.CustomerID, oa.OrderID FROM Customers c CROSS APPLY (SELECT o.OrderID FROM Orders o WHERE o.CustomerID = c.CustomerID) oa WHERE oa.OrderID < 10250",
                "SELECT c.CustomerID, o.OrderID FROM Customers c INNER JOIN Orders o ON o.CustomerID = c.CustomerID WHERE o.OrderID < 10250")]
    // CROSS APPLY over a correlated scalar aggregate: COUNT(*) always yields one row (0 for an
    // employee with no orders), so every left row survives -- same as the scalar subquery form.
    [InlineData("SELECT e.EmployeeID, s.OrderCount FROM Employees e CROSS APPLY (SELECT COUNT(*) AS OrderCount FROM Orders o WHERE o.EmployeeID = e.EmployeeID) s",
                "SELECT e.EmployeeID, (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) AS OrderCount FROM Employees e")]
    // CROSS APPLY whose body holds an EXISTS correlated to BOTH the apply's own table (t) and the
    // apply's left (e). Decorrelating the inner EXISTS yields a hash-match join whose residual
    // tests the outer column e.EmployeeID -- the doubly-correlated case. Equivalent to walking the
    // assignment table directly.
    [InlineData("SELECT e.EmployeeID, x.Territory FROM Employees e CROSS APPLY (SELECT t.TerritoryDescription AS Territory FROM Territories t WHERE EXISTS (SELECT * FROM EmployeeTerritories et WHERE t.TerritoryID = et.TerritoryID AND et.EmployeeID = e.EmployeeID)) x",
                "SELECT e.EmployeeID, t.TerritoryDescription AS Territory FROM Employees e INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID INNER JOIN Territories t ON t.TerritoryID = et.TerritoryID")]
    // OUTER APPLY, uncorrelated: the right is independent of the left and its scalar aggregate
    // always yields exactly one row, so no left row is ever dropped -- the same as a CROSS JOIN.
    [InlineData("SELECT e.EmployeeID, x.OrderCount FROM Employees e OUTER APPLY (SELECT COUNT(*) AS OrderCount FROM Orders o) x",
                "SELECT e.EmployeeID, x.OrderCount FROM Employees e CROSS JOIN (SELECT COUNT(*) AS OrderCount FROM Orders o) x")]
    // OUTER APPLY, correlated to the left: equivalent to a LEFT JOIN on the correlation predicate.
    // The defining difference from CROSS APPLY -- a left row with no matching right rows survives,
    // null-padded -- so customers with no orders still appear (oa.OrderID NULL).
    [InlineData("SELECT c.CustomerID, oa.OrderID FROM Customers c OUTER APPLY (SELECT o.OrderID FROM Orders o WHERE o.CustomerID = c.CustomerID) oa",
                "SELECT c.CustomerID, o.OrderID FROM Customers c LEFT JOIN Orders o ON o.CustomerID = c.CustomerID")]
    // OUTER APPLY whose body further filters the correlated rows: the predicate belongs inside the
    // apply, so the LEFT JOIN oracle carries it in the ON clause (not WHERE, which would drop the
    // preserved null rows). Left rows whose only matches fail the filter still survive.
    [InlineData("SELECT c.CustomerID, oa.OrderID FROM Customers c OUTER APPLY (SELECT o.OrderID FROM Orders o WHERE o.CustomerID = c.CustomerID AND o.OrderID < 10260) oa",
                "SELECT c.CustomerID, o.OrderID FROM Customers c LEFT JOIN Orders o ON o.CustomerID = c.CustomerID AND o.OrderID < 10260")]
    // OUTER APPLY over a correlated scalar aggregate: COUNT(*) yields one row (0 for an employee
    // with no orders), so every left row survives either way -- same as the scalar subquery form,
    // and here indistinguishable from the CROSS APPLY equivalent.
    [InlineData("SELECT e.EmployeeID, s.OrderCount FROM Employees e OUTER APPLY (SELECT COUNT(*) AS OrderCount FROM Orders o WHERE o.EmployeeID = e.EmployeeID) s",
                "SELECT e.EmployeeID, (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) AS OrderCount FROM Employees e")]
    // Set operators.
    [InlineData("SELECT e.City FROM Employees e UNION ALL SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e UNION SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e INTERSECT SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e EXCEPT SELECT c.City FROM Customers c")]
    public void NQuery_ProducesSameRows_AsSqlite(string nqueryText, string? sqliteText = null)
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
    public void NQuery_ProducesRowsInSameOrder_AsSqlite(string nqueryText, string? sqliteText = null)
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
                row[i] = RowSet.Normalize(reader[i])!;
            rows.Add(row);
        }

        return rows;
    }
}
