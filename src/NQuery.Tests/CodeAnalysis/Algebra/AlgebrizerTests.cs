using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;
using NQuery.Northwind;

namespace NQuery.Tests.CodeAnalysis.Algebra;

public class AlgebrizerTests
{
    // Same corpus as the bound-tree verifier: it exercises every relation
    // kind the binder produces.
    [Theory]
    [InlineData("SELECT 1 + 2")]
    [InlineData("SELECT e.FirstName, e.LastName FROM Employees e")]
    [InlineData("SELECT * FROM Employees e WHERE e.City = 'London'")]
    [InlineData("SELECT e.City, COUNT(*) FROM Employees e GROUP BY e.City")]
    [InlineData("SELECT e.City, COUNT(*) AS C FROM Employees e GROUP BY e.City HAVING COUNT(*) > 1")]
    [InlineData("SELECT DISTINCT e.City FROM Employees e")]
    [InlineData("SELECT e.City FROM Employees e ORDER BY e.City")]
    [InlineData("SELECT TOP 3 e.City FROM Employees e ORDER BY e.City")]
    [InlineData("SELECT TOP 3 WITH TIES e.City FROM Employees e ORDER BY e.City")]
    [InlineData("SELECT o.OrderID, od.ProductID FROM Orders o INNER JOIN [Order Details] od ON o.OrderID = od.OrderID")]
    [InlineData("SELECT e.FirstName, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID")]
    [InlineData("SELECT e.City FROM Employees e UNION SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e UNION ALL SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e EXCEPT SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e INTERSECT SELECT c.City FROM Customers c")]
    [InlineData("SELECT d.City FROM (SELECT e.City FROM Employees e) AS d")]
    [InlineData("WITH X AS (SELECT e.City FROM Employees e) SELECT x.City FROM X x")]
    [InlineData("SELECT (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e")]
    [InlineData("SELECT e.FirstName FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)")]
    [InlineData("SELECT e.FirstName FROM Employees e WHERE e.EmployeeID IN (SELECT o.EmployeeID FROM Orders o)")]
    // Expression-shaped queries, to exercise the logical expression translation.
    [InlineData("SELECT CASE WHEN e.City = 'London' THEN 1 ELSE 0 END FROM Employees e")]
    [InlineData("SELECT CAST(e.EmployeeID AS FLOAT) FROM Employees e")]
    [InlineData("SELECT -e.EmployeeID, e.EmployeeID + 1 FROM Employees e")]
    [InlineData("SELECT e.FirstName FROM Employees e WHERE e.ReportsTo IS NULL")]
    [InlineData("SELECT COALESCE(e.Region, 'n/a') FROM Employees e")]
    [InlineData("SELECT SOUNDEX(e.FirstName) FROM Employees e")]
    [InlineData("SELECT e.FirstName.Length FROM Employees e")]
    [InlineData("SELECT e.FirstName.Substring(0, 1) FROM Employees e")]
    public void Algebrizer_FaithfullyTranslates_BinderOutput(string text)
    {
        var boundQuery = Bind(text);
        var logicalQuery = Algebrizer.Algebrize(boundQuery);

        Assert.NotNull(logicalQuery.Root);

        // Algebrization preserves the query's external contract: the logical root
        // produces one output slot per output column, in the same order and of the same
        // type, and exposes the same output columns as the bound query. (The algebrizer
        // now mints its own slots from the binder's value identities, so this is
        // type/arity equality, not slot-reference equality. The internal defined-value
        // scope is not preserved for subqueries -- Apply introduction lifts the
        // subquery's slots into the surrounding scope, which is intended.)
        Assert.Equal(boundQuery.OutputColumns.Select(c => c.Type), logicalQuery.Root.OutputValueSlots.Select(s => s.Type));
        Assert.Equal(boundQuery.OutputColumns, logicalQuery.OutputColumns);
    }

    [Fact]
    public void Algebrizer_IntroducesOuterApply_ForScalarSubquery()
    {
        var text = "SELECT (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e";
        var logicalQuery = Algebrizer.Algebrize(Bind(text));

        var applies = logicalQuery.Root.DescendantsAndSelf().OfType<LogicalApply>().ToList();

        Assert.Single(applies);
        Assert.Equal(LogicalApplyKind.LeftOuter, applies[0].ApplyKind);
        Assert.Null(applies[0].Probe);
    }

    [Fact]
    public void Algebrizer_IntroducesSemiApplyWithProbe_ForExists()
    {
        var text = "SELECT e.FirstName FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)";
        var logicalQuery = Algebrizer.Algebrize(Bind(text));

        var applies = logicalQuery.Root.DescendantsAndSelf().OfType<LogicalApply>().ToList();

        Assert.Single(applies);
        Assert.Equal(LogicalApplyKind.LeftSemi, applies[0].ApplyKind);
        Assert.NotNull(applies[0].Probe);
    }

    [Theory]
    // NOT EXISTS becomes NOT <probe> over a Semi apply; EXISTS embedded in a
    // larger predicate is handled by the same probe mechanism.
    [InlineData("SELECT e.FirstName FROM Employees e WHERE NOT EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)")]
    [InlineData("SELECT e.FirstName FROM Employees e WHERE e.City = 'London' AND EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)")]
    public void Algebrizer_IntroducesApply_ForExistsInPredicate(string text)
    {
        var logicalQuery = Algebrizer.Algebrize(Bind(text));
        Assert.Contains(logicalQuery.Root.DescendantsAndSelf(), o => o is LogicalApply);
    }

    [Fact]
    public void Algebrizer_PushesSubqueryApplyOntoRight_ForOuterJoinCondition()
    {
        // A subquery in a LEFT join's ON can't be hoisted above the join (that would change
        // null-padding), so it is pushed onto the join's right input -- the join condition
        // then references the probe slot the Apply exposes there.
        var text = "SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND EXISTS (SELECT * FROM Customers c WHERE c.CustomerID = o.CustomerID)";
        var logicalQuery = Algebrizer.Algebrize(Bind(text));

        var join = logicalQuery.Root.DescendantsAndSelf().OfType<LogicalJoin>().Single();
        Assert.Equal(LogicalJoinKind.LeftOuter, join.JoinKind);

        // There is exactly one Apply, and it sits inside the join's right subtree (not above
        // the join, where an inner join would have hoisted it).
        var apply = logicalQuery.Root.DescendantsAndSelf().OfType<LogicalApply>().Single();
        Assert.Contains(apply, join.Right.DescendantsAndSelf().OfType<LogicalApply>());
    }

    [Fact]
    public void Algebrizer_LowersAsDependentJoin_ForOuterJoinConditionSubqueryCorrelatedToLeft()
    {
        // Pushing the Apply onto the right can't satisfy a correlation to the join's left
        // (here e.EmployeeID), so the whole join is lowered as a dependent join: a LeftOuter
        // Apply whose left is Employees and whose right (the Orders side + subquery + condition)
        // is correlated back to it. There is no plain LogicalJoin for this shape.
        var text = "SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND EXISTS (SELECT * FROM Orders o2 WHERE o2.EmployeeID = e.EmployeeID)";
        var logicalQuery = Algebrizer.Algebrize(Bind(text));

        var outerApply = logicalQuery.Root.DescendantsAndSelf()
                                     .OfType<LogicalApply>()
                                     .First(a => a.Left is LogicalTableScan { TableInstance.Table.Name: "Employees" });
        Assert.Equal(LogicalApplyKind.LeftOuter, outerApply.ApplyKind);

        // The left (Employees) is referenced from inside the apply's right subtree -- that
        // correlation is exactly what couldn't be satisfied by a right-side push.
        Assert.NotEmpty(outerApply.OuterReferences);
    }

    [Fact]
    public void Algebrizer_ExpandsToUnion_ForFullOuterJoinConditionSubqueryCorrelatedToLeft()
    {
        // FULL OUTER has no single Apply form, so a left-correlated subquery in its ON is
        // expanded into (LEFT OUTER) UNION ALL (right-anti-semi), each branch built as a
        // dependent join. There is no LogicalJoin with FullOuter kind left in the tree.
        var text = "SELECT e.EmployeeID, o.OrderID FROM Employees e FULL JOIN Orders o ON e.EmployeeID = o.EmployeeID AND EXISTS (SELECT * FROM Orders o2 WHERE o2.EmployeeID = e.EmployeeID)";
        var logicalQuery = Algebrizer.Algebrize(Bind(text));

        var nodes = logicalQuery.Root.DescendantsAndSelf().ToArray();
        Assert.Contains(nodes, o => o is LogicalUnion { IsUnionAll: true });
        Assert.DoesNotContain(nodes, o => o is LogicalJoin { JoinKind: LogicalJoinKind.FullOuter });
        // Both an outer and an anti-semi dependent join (one per branch).
        Assert.Contains(nodes, o => o is LogicalApply { ApplyKind: LogicalApplyKind.LeftOuter });
        Assert.Contains(nodes, o => o is LogicalApply { ApplyKind: LogicalApplyKind.LeftAntiSemi });
    }

    [Fact]
    public void Algebrizer_NormalizesRightOuterJoin_ToLeftOuter()
    {
        var text = "SELECT e.FirstName FROM Orders o RIGHT JOIN Employees e ON e.EmployeeID = o.EmployeeID";
        var logicalQuery = Algebrizer.Algebrize(Bind(text));

        var join = logicalQuery.Root.DescendantsAndSelf().OfType<LogicalJoin>().Single();
        Assert.Equal(LogicalJoinKind.LeftOuter, join.JoinKind);

        // The preserved side (Employees) was swapped to the left.
        var leftScan = Assert.IsType<LogicalTableScan>(join.Left);
        Assert.Equal("Employees", leftScan.TableInstance.Table.Name);
    }

    private static BoundQuery Bind(string text)
    {
        var syntaxTree = SyntaxTree.ParseQuery(text);
        var bindingResult = Binder.Bind(syntaxTree.Root, NorthwindCatalog.Instance);
        Assert.Empty(syntaxTree.GetDiagnostics().Concat(bindingResult.Diagnostics));
        return (BoundQuery)bindingResult.BoundRoot;
    }
}
