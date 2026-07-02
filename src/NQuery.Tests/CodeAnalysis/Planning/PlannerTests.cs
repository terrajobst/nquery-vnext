using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;
using NQuery.CodeAnalysis.Optimization;
using NQuery.CodeAnalysis.Planning;
using NQuery.Northwind;

namespace NQuery.Tests.CodeAnalysis.Planning;

public class PlannerTests
{
    [Theory]
    [InlineData("SELECT e.FirstName FROM Employees e")]
    [InlineData("SELECT e.City, COUNT(*) FROM Employees e GROUP BY e.City")]
    [InlineData("SELECT o.OrderID FROM Orders o INNER JOIN [Order Details] od ON o.OrderID = od.OrderID")]
    [InlineData("SELECT e.LastName, c.City FROM Employees e, Customers c")]
    [InlineData("SELECT e.City FROM Employees e UNION SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.FirstName FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)")]
    public void Planner_PreservesOutput(string text)
    {
        var logicalQuery = LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text)), NorthwindCatalog.Instance);
        var physicalQuery = Planner.Plan(logicalQuery);

        // Planning chooses algorithms; it does not change the data flow.
        Assert.Equal(logicalQuery.Root.OutputValueSlots, physicalQuery.Root.OutputValueSlots);
        Assert.Equal(logicalQuery.OutputColumns, physicalQuery.OutputColumns);
    }

    [Fact]
    public void Planner_BuildsHashMatch_ForEquiJoin()
    {
        var text = "SELECT o.OrderID FROM Orders o INNER JOIN [Order Details] od ON o.OrderID = od.OrderID";
        var hashMatch = Plan(text).DescendantsAndSelf().OfType<PhysicalHashMatch>().Single();

        Assert.Equal(PhysicalHashMatchKind.Inner, hashMatch.HashMatchKind);
    }

    [Fact]
    public void Planner_BuildsHashMatch_ForComputedEquiKey()
    {
        // ON e.EmployeeID + 0 = o.EmployeeID isn't slot = slot, so the planner
        // materializes the computed side into a key slot (a compute on that input) and
        // hash-matches on it.
        var text = "SELECT e.FirstName FROM Employees e INNER JOIN Orders o ON e.EmployeeID + 0 = o.EmployeeID";
        var hashMatch = Plan(text).DescendantsAndSelf().OfType<PhysicalHashMatch>().Single();

        Assert.Equal(PhysicalHashMatchKind.Inner, hashMatch.HashMatchKind);
    }

    [Fact]
    public void Planner_BuildsHashMatch_ForEquiExists()
    {
        // A correlated EXISTS decorrelates to a probing left-semi join on an equi key,
        // so it becomes a (semi) hash match rather than a nested-loops re-scan. The
        // probe column carries the per-row match marker the enclosing filter tests.
        var text = "SELECT e.FirstName FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)";
        var hashMatch = Plan(text).DescendantsAndSelf().OfType<PhysicalHashMatch>().Single();

        Assert.Equal(PhysicalHashMatchKind.LeftSemi, hashMatch.HashMatchKind);
        Assert.NotNull(hashMatch.ProbeColumn);
    }

    [Fact]
    public void Planner_BuildsNestedLoops_ForNonEquiJoin()
    {
        var text = "SELECT o.OrderID FROM Orders o INNER JOIN [Order Details] od ON o.OrderID <> od.OrderID";
        var join = Plan(text).DescendantsAndSelf().OfType<PhysicalNestedLoops>().Single();

        Assert.NotEmpty(join.Conditions);
    }

    [Fact]
    public void Planner_BuildsNestedLoops_ForCrossProduct()
    {
        var text = "SELECT e.LastName, c.City FROM Employees e, Customers c";
        var join = Plan(text).DescendantsAndSelf().OfType<PhysicalNestedLoops>().Single();

        Assert.Empty(join.Conditions);
    }

    [Fact]
    public void Planner_BuildsHashMatch_ForEquiFullOuterJoin()
    {
        // A hash match produces FULL OUTER directly, so an equi full outer needs no
        // expansion.
        var text = "SELECT e.City, c.City FROM Employees e FULL JOIN Customers c ON e.City = c.City";
        var hashMatch = Plan(text).DescendantsAndSelf().OfType<PhysicalHashMatch>().Single();

        Assert.Equal(PhysicalHashMatchKind.FullOuter, hashMatch.HashMatchKind);
    }

    [Fact]
    public void Planner_ExpandsNonEquiFullOuterJoin_IntoConcatenation()
    {
        // A non-equi full outer can't use a hash match, so the planner expands it into
        // (left outer) UNION ALL (right-anti-semi with the left null-padded).
        var text = "SELECT e.City, c.City FROM Employees e FULL JOIN Customers c ON e.City <> c.City";
        var plan = Plan(text);

        Assert.NotEmpty(plan.DescendantsAndSelf().OfType<PhysicalConcatenation>());
        Assert.Contains(plan.DescendantsAndSelf().OfType<PhysicalNestedLoops>(), j => j.JoinKind == PhysicalJoinKind.LeftAntiSemi);
        Assert.Contains(plan.DescendantsAndSelf().OfType<PhysicalNestedLoops>(), j => j.JoinKind == PhysicalJoinKind.LeftOuter);
    }

    [Fact]
    public void Planner_BuildsIndexSpool_ForCorrelatedEqualityFilter()
    {
        // The correlated scalar TOP 1 subquery has no decorrelation rule, so it
        // survives as an apply; its correlated equality filter over an independent
        // input becomes an index spool -- one scan of Orders, probed per customer --
        // instead of a per-customer re-scan.
        var text = """
            SELECT  c.CustomerID,
                    (SELECT TOP 1 o.OrderID FROM Orders o WHERE o.CustomerID = c.CustomerID ORDER BY o.OrderDate)
            FROM    Customers c
            """;
        var spool = Plan(text).DescendantsAndSelf().OfType<PhysicalIndexSpool>().Single();

        Assert.Contains(spool.IndexKey, spool.Input.OutputValueSlots);
    }

    [Fact]
    public void Planner_BuildsIndexSpool_ForComputedIndexKey()
    {
        // The inner side of the equality is an expression, so the planner materializes
        // it with a compute below the spool (like the hash match's computed key) and
        // projects the extra slot away above it.
        var text = """
            SELECT  e.EmployeeID,
                    (SELECT TOP 1 o.OrderID FROM Orders o WHERE o.EmployeeID + 1 = e.EmployeeID ORDER BY o.OrderDate)
            FROM    Employees e
            """;
        var plan = Plan(text);
        var spool = plan.DescendantsAndSelf().OfType<PhysicalIndexSpool>().Single();

        Assert.IsType<PhysicalComputeScalar>(spool.Input);
    }

    [Fact]
    public void Planner_KeepsCorrelatedResidual_AboveIndexSpool()
    {
        // Only the equality is consumed by the spool; the other correlated conjunct
        // survives as a filter above it, evaluated per row on the probed subset.
        var text = """
            SELECT  e.EmployeeID,
                    (SELECT TOP 1 o.OrderID FROM Orders o WHERE o.EmployeeID = e.EmployeeID AND o.Freight > e.EmployeeID ORDER BY o.OrderDate)
            FROM    Employees e
            """;
        var plan = Plan(text);
        var spool = plan.DescendantsAndSelf().OfType<PhysicalIndexSpool>().Single();
        var residual = plan.DescendantsAndSelf().OfType<PhysicalFilter>().Single(f => f.Input == spool);

        Assert.Single(residual.Conditions);
    }

    [Fact]
    public void Planner_BuildsNoIndexSpool_WhenFilterInputIsCorrelated()
    {
        // The equality's input subtree is itself correlated (the TOP 3 derived table
        // filters on the outer employee), so a build-once spool would serve stale rows;
        // the filter stays a plain correlated filter. (The inner TOP blocks selection
        // pushdown from merging the two predicates into one filter over the scan.)
        var text = """
            SELECT  e.EmployeeID,
                    (SELECT  TOP 1 o.OrderID
                     FROM    (SELECT TOP 3 o2.OrderID, o2.EmployeeID FROM Orders o2 WHERE o2.Freight > e.EmployeeID ORDER BY o2.OrderID) o
                     WHERE   o.EmployeeID = e.EmployeeID
                     ORDER   BY o.OrderID)
            FROM    Employees e
            """;
        var plan = Plan(text);

        Assert.Empty(plan.DescendantsAndSelf().OfType<PhysicalIndexSpool>());
    }

    private static PhysicalOperator Plan(string text)
    {
        return Planner.Plan(LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text)), NorthwindCatalog.Instance)).Root;
    }

    private static BoundQuery Bind(string text)
    {
        var syntaxTree = SyntaxTree.ParseQuery(text);
        var bindingResult = Binder.Bind(syntaxTree.Root, NorthwindCatalog.Instance);
        Assert.Empty(syntaxTree.GetDiagnostics().Concat(bindingResult.Diagnostics));
        return (BoundQuery)bindingResult.BoundRoot;
    }
}
