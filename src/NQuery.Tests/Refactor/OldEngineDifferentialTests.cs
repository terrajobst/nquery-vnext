using System.Collections.Generic;

namespace NQuery.Tests.Refactor
{
    // Differential correctness gate against the OLD engine: each query runs through both the
    // live engine and the previous-version NQuery from the "nquery-baseline" worktree, over the
    // same Northwind data, and the result rows must match. This is the cross-version analogue of
    // EmitterExecutionTests.NewPipeline_ProducesSameRows_AsUnoptimized, which compares the new
    // pipeline to the in-tree unoptimized reference; here the reference is the shipped engine at
    // the comparison commit.
    //
    // The old engine is reached through the OldEngine helper, which keeps the extern alias to
    // itself, so this file speaks only standard types. Every query carries an ORDER BY (or is a
    // single-row aggregate) so the row order is deterministic and the comparison is positional.
    public class OldEngineDifferentialTests
    {
        [Theory]
        // Scan + filter + project.
        [InlineData("SELECT e.EmployeeID, e.City FROM Employees e WHERE e.Country = 'UK' ORDER BY e.EmployeeID")]
        [InlineData("SELECT e.EmployeeID, e.EmployeeID + 1 FROM Employees e ORDER BY e.EmployeeID")]
        // NULL handling and CASE.
        [InlineData("SELECT e.EmployeeID FROM Employees e WHERE e.ReportsTo IS NULL ORDER BY e.EmployeeID")]
        [InlineData("SELECT e.EmployeeID, CASE WHEN e.City = 'London' THEN 1 ELSE 0 END FROM Employees e ORDER BY e.EmployeeID")]
        // Sort + TOP.
        [InlineData("SELECT TOP 3 e.City FROM Employees e ORDER BY e.City, e.EmployeeID")]
        // Inner join.
        [InlineData("SELECT o.OrderID, e.City FROM Employees e INNER JOIN Orders o ON e.EmployeeID = o.EmployeeID WHERE o.OrderID < 10260 ORDER BY o.OrderID")]
        // Left outer join with NULL padding (only one order matches, the rest pad).
        [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND o.OrderID = 10248 ORDER BY e.EmployeeID, o.OrderID")]
        // Correlated EXISTS / NOT EXISTS.
        [InlineData("SELECT e.EmployeeID FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID) ORDER BY e.EmployeeID")]
        [InlineData("SELECT c.CustomerID FROM Customers c WHERE NOT EXISTS (SELECT * FROM Orders o WHERE o.CustomerID = c.CustomerID) ORDER BY c.CustomerID")]
        // IN over a subquery.
        [InlineData("SELECT e.EmployeeID, e.ReportsTo FROM Employees e WHERE e.ReportsTo IN (SELECT o.EmployeeID FROM Orders o) ORDER BY e.EmployeeID")]
        // Correlated scalar subquery (COUNT must be 0, not NULL, for employees with no orders).
        [InlineData("SELECT e.EmployeeID, (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e ORDER BY e.EmployeeID")]
        // Scalar aggregate over all rows (single row, so order is irrelevant).
        [InlineData("SELECT COUNT(*), MAX(e.EmployeeID), MIN(e.EmployeeID), SUM(e.EmployeeID) FROM Employees e")]
        // Grouped aggregate, including a NULL-bearing grouping column and a multi-column grouping.
        [InlineData("SELECT e.City, COUNT(*) FROM Employees e GROUP BY e.City ORDER BY e.City")]
        [InlineData("SELECT e.ReportsTo, COUNT(*) FROM Employees e GROUP BY e.ReportsTo ORDER BY e.ReportsTo")]
        [InlineData("SELECT e.Country, e.City, COUNT(*) FROM Employees e GROUP BY e.Country, e.City ORDER BY e.Country, e.City")]
        // Set operators.
        [InlineData("SELECT e.City FROM Employees e UNION ALL SELECT c.City FROM Customers c ORDER BY 1")]
        [InlineData("SELECT e.City FROM Employees e UNION SELECT c.City FROM Customers c ORDER BY 1")]
        [InlineData("SELECT e.City FROM Employees e INTERSECT SELECT c.City FROM Customers c ORDER BY 1")]
        [InlineData("SELECT e.City FROM Employees e EXCEPT SELECT c.City FROM Customers c ORDER BY 1")]
        public void NewEngine_ProducesSameRows_AsOldEngine(string text)
        {
            var expected = OldEngine.RunQuery(text);
            var actual = RunNew(text);

            Assert.Equal(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
                Assert.Equal(expected[i], actual[i]);
        }

        private static List<object[]> RunNew(string text)
        {
            using var reader = Query.Create(NorthwindDataContext.Instance, text).ExecuteReader();

            var rows = new List<object[]>();
            while (reader.Read())
            {
                var row = new object[reader.ColumnCount];
                for (var i = 0; i < row.Length; i++)
                    row[i] = reader[i];
                rows.Add(row);
            }

            return rows;
        }
    }
}
