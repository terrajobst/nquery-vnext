using NQuery.Northwind;

namespace NQuery.Tests.Evaluation;

// End-to-end evaluation of correlated subqueries the planner lowers to an index
// spool (see Planner.TryPlanIndexSpool): a correlated TOP 1 survives decorrelation
// as an apply, and its equality filter becomes a build-once, probe-per-row spool.
// These tests pin the equality semantics the spool must preserve -- a NULL probe
// or a NULL index key never matches -- and compare against equivalent queries that
// take entirely different plan shapes.
//
// Northwind employee hierarchy: employee 2 is the root (ReportsTo IS NULL); 1, 3,
// 4, 5, and 8 report to 2; 6, 7, and 9 report to 5.
public class IndexSpoolTests : EvaluationTest
{
    [Fact]
    public void Evaluation_IndexSpool_NullProbe_MatchesNothing()
    {
        // The probe is e.ReportsTo, which is NULL for the boss -- the subquery must
        // yield NULL for that row, not match anything.
        var text = """
            SELECT  e.EmployeeID,
                    (SELECT TOP 1 e2.EmployeeID FROM Employees e2 WHERE e2.EmployeeID = e.ReportsTo ORDER BY e2.EmployeeID)
            FROM    Employees e
            ORDER   BY 1
            """;

        var equivalent = """
            SELECT  e.EmployeeID,
                    e.ReportsTo
            FROM    Employees e
            ORDER   BY 1
            """;

        AssertProducesSameAs(text, equivalent);
    }

    [Fact]
    public void Evaluation_IndexSpool_NullIndexKey_MatchesNothing()
    {
        // The index key is e2.ReportsTo, which is NULL for the boss -- that row must
        // not be indexed under any key. The equivalent computes the same "first direct
        // report" through a group-by and hash join (a completely different plan).
        var text = """
            SELECT  e.EmployeeID,
                    (SELECT TOP 1 e2.EmployeeID FROM Employees e2 WHERE e2.ReportsTo = e.EmployeeID ORDER BY e2.EmployeeID)
            FROM    Employees e
            ORDER   BY 1
            """;

        var equivalent = """
            SELECT  e.EmployeeID,
                    d.FirstReport
            FROM    Employees e
                        LEFT JOIN (SELECT e2.ReportsTo AS Manager, MIN(e2.EmployeeID) AS FirstReport
                                   FROM Employees e2
                                   GROUP BY e2.ReportsTo) d ON d.Manager = e.EmployeeID
            ORDER   BY 1
            """;

        AssertProducesSameAs(text, equivalent);
    }

    [Fact]
    public void Evaluation_IndexSpool_EmitsAllMatches_PerOuterRow()
    {
        // TOP 2 blocks decorrelation, so the apply survives and the spool serves
        // multiple rows per probe, re-probed per outer row. The expectation -- the two
        // smallest order ids per employee -- is computed straight from the data.
        var text = """
            SELECT  e.EmployeeID,
                    d.OrderID
            FROM    Employees e
                        CROSS APPLY (SELECT TOP 2 o.OrderID FROM Orders o WHERE o.EmployeeID = e.EmployeeID ORDER BY o.OrderID) d
            ORDER   BY 1, 2
            """;

        var expected = NorthwindData.Instance.Employees
            .OrderBy(e => e.EmployeeID)
            .SelectMany(e => NorthwindData.Instance.Orders
                                          .Where(o => o.EmployeeID == e.EmployeeID)
                                          .OrderBy(o => o.OrderID)
                                          .Take(2),
                        (e, o) => (e.EmployeeID, o.OrderID))
            .ToArray();

        Assert.NotEmpty(expected);
        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_IndexSpool_CorrelatedResidual_IsAppliedPerRow()
    {
        // The second, non-equality conjunct stays above the spool and must still be
        // evaluated against every outer row.
        var text = """
            SELECT  e.EmployeeID,
                    (SELECT TOP 1 o.OrderID
                     FROM   Orders o
                     WHERE  o.EmployeeID = e.EmployeeID AND o.OrderID % 2 = e.EmployeeID % 2
                     ORDER  BY o.OrderID)
            FROM    Employees e
            ORDER   BY 1
            """;

        var equivalent = """
            SELECT  e.EmployeeID,
                    (SELECT MIN(o.OrderID)
                     FROM   Orders o
                     WHERE  o.EmployeeID = e.EmployeeID AND o.OrderID % 2 = e.EmployeeID % 2)
            FROM    Employees e
            ORDER   BY 1
            """;

        AssertProducesSameAs(text, equivalent);
    }

    [Fact]
    public void Evaluation_IndexSpool_ComputedProbe_MatchesPlainEquivalent()
    {
        // The probe is a computed outer expression (e.EmployeeID + 1): for each employee,
        // the first order of the *next* employee. SpoolProbeHoist materializes the probe
        // onto the outer side so the spool can key on it. The equivalent computes the same
        // "first order per employee" through a group-by and a computed-key hash join (a
        // completely different plan). Employee 9 has no successor, so it yields NULL both
        // ways.
        var text = """
            SELECT  e.EmployeeID,
                    (SELECT TOP 1 o.OrderID FROM Orders o WHERE o.EmployeeID = e.EmployeeID + 1 ORDER BY o.OrderID)
            FROM    Employees e
            ORDER   BY 1
            """;

        var equivalent = """
            SELECT  e.EmployeeID,
                    d.FirstOrder
            FROM    Employees e
                        LEFT JOIN (SELECT e2.EmployeeID AS Emp, MIN(o.OrderID) AS FirstOrder
                                   FROM Employees e2
                                            INNER JOIN Orders o ON o.EmployeeID = e2.EmployeeID
                                   GROUP BY e2.EmployeeID) d ON d.Emp = e.EmployeeID + 1
            ORDER   BY 1
            """;

        AssertProducesSameAs(text, equivalent);
    }

    // Runs both queries and asserts they produce the same rows in the same order.
    private static void AssertProducesSameAs(string text, string equivalentText)
    {
        var expected = ReadRows(equivalentText);
        var actual = ReadRows(text);

        Assert.NotEmpty(expected);
        Assert.Equal(expected, actual);
    }

    private static List<object?[]> ReadRows(string text)
    {
        using var reader = Query.Create(NorthwindCatalog.Instance, text).ExecuteReader();

        var rows = new List<object?[]>();
        while (reader.Read())
        {
            var row = new object?[reader.ColumnCount];
            for (var i = 0; i < row.Length; i++)
                row[i] = reader[i];
            rows.Add(row);
        }

        return rows;
    }
}
