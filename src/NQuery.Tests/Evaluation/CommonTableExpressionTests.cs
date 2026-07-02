using NQuery.Northwind;

namespace NQuery.Tests.Evaluation;

// End-to-end evaluation of common table expressions -- the algebrizer inlines
// non-recursive CTEs per reference and lowers recursive ones to the working-table
// recursion (see REFACTORING.md, "CTE Design"). Where a CTE has a straightforward
// hand-inlined equivalent, the test also asserts both produce the same rows.
//
// The Northwind employee hierarchy used by the recursive tests: employee 2 is the
// root (ReportsTo IS NULL); 1, 3, 4, 5, and 8 report to 2; 6, 7, and 9 report to 5.
public class CommonTableExpressionTests : EvaluationTest
{
    [Fact]
    public void Evaluation_Cte_SingleReference()
    {
        var text = @"
            WITH LowIds AS (
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.EmployeeID <= 3
            )
            SELECT  t.EmployeeID
            FROM    LowIds t
            ORDER   BY 1
        ";

        var expected = new[] { 1, 2, 3 };

        AssertProduces(text, expected);
        AssertProducesSameAs(text, @"
            SELECT  e.EmployeeID
            FROM    Employees e
            WHERE   e.EmployeeID <= 3
            ORDER   BY 1
        ");
    }

    [Fact]
    public void Evaluation_Cte_ColumnList()
    {
        var text = @"
            WITH E (Id, Name) AS (
                SELECT  e.EmployeeID,
                        e.FirstName
                FROM    Employees e
            )
            SELECT  E.Id
            FROM    E
            WHERE   E.Name = 'Andrew'
        ";

        var expected = new[] { 2 };

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_StarExpansion()
    {
        var text = @"
            WITH E AS (
                SELECT  *
                FROM    Employees e
                WHERE   e.City = 'London'
            )
            SELECT  E.EmployeeID
            FROM    E
            ORDER   BY 1
        ";

        var expected = new[] { 5, 6, 7, 9 };

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_MultipleReferences()
    {
        // Each reference is a separate, slot-disjoint instantiation of the CTE's body.
        var text = @"
            WITH E AS (
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.EmployeeID <= 4
            )
            SELECT  l.EmployeeID,
                    r.EmployeeID
            FROM    E l
                        INNER JOIN E r ON l.EmployeeID = r.EmployeeID + 1
            ORDER   BY 1
        ";

        var expected = new[] { (2, 1), (3, 2), (4, 3) };

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_ChainedReferences()
    {
        // A CTE can reference every CTE defined before it; the inner reference is
        // inlined inside the outer CTE's instantiation.
        var text = @"
            WITH LondonEmployees AS (
                SELECT  *
                FROM    Employees e
                WHERE   e.City = 'London'
            ), LateLondonEmployees AS (
                SELECT  le.EmployeeID
                FROM    LondonEmployees le
                WHERE   le.EmployeeID > 5
            )
            SELECT  t.EmployeeID
            FROM    LateLondonEmployees t
            ORDER   BY 1
        ";

        var expected = new[] { 6, 7, 9 };

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_InSubquery()
    {
        var text = @"
            WITH LondonEmployees AS (
                SELECT  *
                FROM    Employees e
                WHERE   e.City = 'London'
            )
            SELECT  e.EmployeeID
            FROM    Employees e
            WHERE   e.EmployeeID IN (SELECT le.EmployeeID FROM LondonEmployees le)
                    AND EXISTS (SELECT * FROM LondonEmployees le2 WHERE le2.EmployeeID = e.EmployeeID)
            ORDER   BY 1
        ";

        var expected = new[] { 5, 6, 7, 9 };

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_Aggregation()
    {
        var text = @"
            WITH E AS (
                SELECT  *
                FROM    Employees e
            )
            SELECT  COUNT(*)
            FROM    E
        ";

        var expected = new[] { 9 };

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_Recursive()
    {
        var text = @"
            WITH EmployeeHierarchy AS (
                SELECT  e.EmployeeID,
                        0 AS Level
                FROM    Employees e
                WHERE   e.ReportsTo IS NULL

                UNION ALL

                SELECT  e.EmployeeID,
                        eh.Level + 1 AS Level
                FROM    Employees e
                            INNER JOIN EmployeeHierarchy eh ON e.ReportsTo = eh.EmployeeID
            )
            SELECT  eh.EmployeeID,
                    eh.Level
            FROM    EmployeeHierarchy eh
            ORDER   BY eh.Level, eh.EmployeeID
        ";

        var expected = new[]
        {
            (2, 0),
            (1, 1), (3, 1), (4, 1), (5, 1), (8, 1),
            (6, 2), (7, 2), (9, 2)
        };

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_Recursive_MultipleAnchorsAndMembers()
    {
        // Two anchors seed the boss twice; two identical recursive members double each
        // round's expansion, so level N contributes 2^(N+1) copies of each employee.
        var text = @"
            WITH EmployeeHierarchy AS (
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.ReportsTo IS NULL

                UNION ALL

                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.ReportsTo IS NULL

                UNION ALL

                SELECT  e.EmployeeID
                FROM    Employees e
                            INNER JOIN EmployeeHierarchy eh ON e.ReportsTo = eh.EmployeeID

                UNION ALL

                SELECT  e.EmployeeID
                FROM    Employees e
                            INNER JOIN EmployeeHierarchy eh ON e.ReportsTo = eh.EmployeeID
            )
            SELECT  COUNT(*)
            FROM    EmployeeHierarchy
        ";

        // Level 0: the boss, twice. Level 1: five direct reports, 2 anchors x 2 members
        // = 4 copies each. Level 2: three, 8 copies each.
        var expected = new[] { 2 + 5 * 4 + 3 * 8 };

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_Recursive_MultipleReferences()
    {
        // The recursive union and its self-references are cloned as one unit per
        // outer reference.
        var text = @"
            WITH EmployeeHierarchy AS (
                SELECT  e.EmployeeID,
                        0 AS Level
                FROM    Employees e
                WHERE   e.ReportsTo IS NULL

                UNION ALL

                SELECT  e.EmployeeID,
                        eh.Level + 1 AS Level
                FROM    Employees e
                            INNER JOIN EmployeeHierarchy eh ON e.ReportsTo = eh.EmployeeID
            )
            SELECT  l.EmployeeID,
                    r.Level
            FROM    EmployeeHierarchy l
                        INNER JOIN EmployeeHierarchy r ON l.EmployeeID = r.EmployeeID
            ORDER   BY r.Level, l.EmployeeID
        ";

        var expected = new[]
        {
            (2, 0),
            (1, 1), (3, 1), (4, 1), (5, 1), (8, 1),
            (6, 2), (7, 2), (9, 2)
        };

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_Recursive_FullOuterJoinExpansion()
    {
        // A non-equi FULL OUTER JOIN is expanded by the planner into two branches, each
        // a slot-disjoint clone of both inputs -- which clones the recursive unions and
        // their back-edge references as one unit.
        var text = @"
            WITH EmployeeHierarchy AS (
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.ReportsTo IS NULL

                UNION ALL

                SELECT  e.EmployeeID
                FROM    Employees e
                            INNER JOIN EmployeeHierarchy eh ON e.ReportsTo = eh.EmployeeID
            )
            SELECT  COUNT(*)
            FROM    EmployeeHierarchy l
                        FULL OUTER JOIN EmployeeHierarchy r ON l.EmployeeID < r.EmployeeID
        ";

        // 9 distinct employees per side: 36 pairs with l < r, plus one unmatched row on
        // each side (l = 9 and r = 1).
        var expected = new[] { 36 + 1 + 1 };

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_Recursive_EmptyAnchor()
    {
        // An anchor producing no rows terminates the recursion immediately.
        var text = @"
            WITH EmployeeHierarchy AS (
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.EmployeeID < 0

                UNION ALL

                SELECT  e.EmployeeID
                FROM    Employees e
                            INNER JOIN EmployeeHierarchy eh ON e.ReportsTo = eh.EmployeeID
            )
            SELECT  eh.EmployeeID
            FROM    EmployeeHierarchy eh
        ";

        var expected = Array.Empty<int>();

        AssertProduces(text, expected);
    }

    [Fact]
    public void Evaluation_Cte_Recursive_ExceedingMaxRecursionLevel_Throws()
    {
        // The self-join never shrinks the frontier, so the recursion hits the
        // MAXRECURSION default (100), matching SQL Server and the legacy engine.
        var text = @"
            WITH EmployeeHierarchy AS (
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.ReportsTo IS NULL

                UNION ALL

                SELECT  e.EmployeeID
                FROM    Employees e
                            INNER JOIN EmployeeHierarchy eh ON e.EmployeeID = eh.EmployeeID
            )
            SELECT  eh.EmployeeID
            FROM    EmployeeHierarchy eh
        ";

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            using var reader = Query.Create(NorthwindCatalog.Instance, text).ExecuteReader();
            while (reader.Read())
            {
            }
        });

        Assert.Equal("The maximum recursion 100 has been exhausted before statement completion.", exception.Message);
    }

    // Runs both queries and asserts they produce the same rows in the same order --
    // used to compare a CTE against its hand-inlined equivalent.
    private static void AssertProducesSameAs(string text, string equivalentText)
    {
        var expected = ReadRows(equivalentText);
        var actual = ReadRows(text);

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
