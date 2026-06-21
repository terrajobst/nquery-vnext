using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;
using NQuery.CodeAnalysis.Emit;
using NQuery.CodeAnalysis.Iterators;
using NQuery.CodeAnalysis.Optimization;
using NQuery.CodeAnalysis.Planning;
using NQuery.Northwind;

namespace NQuery.Tests.CodeAnalysis.Emit;

// End-to-end execution through the pipeline
// (Bind -> Algebrize -> Optimize -> Plan -> Emit -> CreateIterator), checked
// differentially against the unoptimized pipeline (RunUnoptimized) -- the nested-loops
// reference that skips logical optimization, an independent oracle for the optimized
// plan. Covers the operators the Emitter wires so far: scan, filter, compute, project,
// sort, top, and nested-loops joins (inner, cross, left outer, and probing semi via
// EXISTS / NOT EXISTS). Queries carry an ORDER BY so the row order is deterministic
// across both pipelines.
public class EmitterExecutionTests
{
    [Theory]
    [InlineData("SELECT e.City FROM Employees e WHERE e.City = 'London'")]
    [InlineData("SELECT e.FirstName, e.EmployeeID + 1 FROM Employees e")]
    [InlineData("SELECT e.FirstName FROM Employees e WHERE e.ReportsTo IS NULL")]
    [InlineData("SELECT CASE WHEN e.City = 'London' THEN 1 ELSE 0 END FROM Employees e")]
    [InlineData("SELECT e.City FROM Employees e ORDER BY e.City")]
    [InlineData("SELECT TOP 3 e.City FROM Employees e ORDER BY e.City")]
    // Inner join with a side-local filter that pushes under the join.
    [InlineData("SELECT od.ProductID, o.CustomerID FROM Orders o INNER JOIN [Order Details] od ON o.OrderID = od.OrderID WHERE o.OrderID = 10248 ORDER BY od.ProductID")]
    // Cross join; SELECT order differs from FROM order to check slot mapping.
    [InlineData("SELECT c.CustomerID, e.EmployeeID FROM Employees e, Customers c ORDER BY e.EmployeeID, c.CustomerID")]
    // Left outer join that leaves most outer rows with a NULL right side.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND o.OrderID = 10248 ORDER BY e.EmployeeID, o.OrderID")]
    // Correlated EXISTS -> probing left-semi join (probe = true path).
    [InlineData("SELECT e.EmployeeID FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID) ORDER BY e.EmployeeID")]
    // Correlated NOT EXISTS -> probing left-semi join (probe = false path).
    [InlineData("SELECT c.CustomerID FROM Customers c WHERE NOT EXISTS (SELECT * FROM Orders o WHERE o.CustomerID = c.CustomerID) ORDER BY c.CustomerID")]
    // EXISTS with both an equi correlation (the hash key) and a cross-input non-equi
    // correlation (the semi hash match's residual remainder).
    [InlineData("SELECT e.EmployeeID FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID AND o.Freight > e.EmployeeID) ORDER BY e.EmployeeID")]
    // IN over an equi key -> a (probing) semi hash match through the live engine; the
    // outer's row order must survive the build flush.
    [InlineData("SELECT e.EmployeeID, e.ReportsTo FROM Employees e WHERE e.ReportsTo IN (SELECT o.EmployeeID FROM Orders o) ORDER BY e.EmployeeID")]
    // Correlated scalar subquery that survives decorrelation (Top blocks it) ->
    // a left-outer apply executed as correlated nested loops.
    [InlineData("SELECT e.EmployeeID, (SELECT TOP 1 o.OrderID FROM Orders o WHERE o.EmployeeID = e.EmployeeID ORDER BY o.OrderID) FROM Employees e ORDER BY e.EmployeeID")]
    // Scalar aggregate over all rows (no GROUP BY) -> stream aggregate, one row.
    [InlineData("SELECT COUNT(*) FROM Employees e")]
    [InlineData("SELECT COUNT(*), MAX(e.EmployeeID), MIN(e.EmployeeID), SUM(e.EmployeeID) FROM Employees e")]
    // Scalar aggregate over an empty input still yields a single row.
    [InlineData("SELECT COUNT(*), SUM(e.EmployeeID) FROM Employees e WHERE e.City = 'Nowhere'")]
    // Grouped aggregate -> sort on the grouping column feeding a stream aggregate.
    [InlineData("SELECT e.City, COUNT(*) FROM Employees e GROUP BY e.City ORDER BY e.City")]
    [InlineData("SELECT e.Country, e.City, COUNT(*) FROM Employees e GROUP BY e.Country, e.City ORDER BY e.Country, e.City")]
    // Grouping column that contains NULLs (ReportsTo) forms its own group.
    [InlineData("SELECT e.ReportsTo, COUNT(*) FROM Employees e GROUP BY e.ReportsTo ORDER BY e.ReportsTo")]
    // Aggregate argument that skips NULLs (COUNT(col)) vs COUNT(*).
    [InlineData("SELECT e.City, COUNT(e.ReportsTo), COUNT(*) FROM Employees e GROUP BY e.City ORDER BY e.City")]
    // UNION ALL -> concatenation: every row from both inputs, duplicates kept.
    [InlineData("SELECT e.City FROM Employees e UNION ALL SELECT c.City FROM Customers c ORDER BY 1")]
    // UNION -> concatenation under a distinct sort, duplicate cities collapsed.
    [InlineData("SELECT e.City FROM Employees e UNION SELECT c.City FROM Customers c ORDER BY 1")]
    // Three-way UNION ALL with differing column sources to check per-input projection.
    [InlineData("SELECT e.EmployeeID FROM Employees e UNION ALL SELECT o.OrderID FROM Orders o UNION ALL SELECT od.ProductID FROM [Order Details] od ORDER BY 1")]
    // INTERSECT -> distinct sort on the left + left-semi join; cities in both tables.
    [InlineData("SELECT e.City FROM Employees e INTERSECT SELECT c.City FROM Customers c ORDER BY 1")]
    // EXCEPT -> distinct sort on the left + left-anti-semi join; cities only employees have.
    [InlineData("SELECT e.City FROM Employees e EXCEPT SELECT c.City FROM Customers c ORDER BY 1")]
    // INTERSECT/EXCEPT over a NULL-bearing column to exercise NULL-equals-NULL matching.
    [InlineData("SELECT e.ReportsTo FROM Employees e EXCEPT SELECT o.EmployeeID FROM Orders o ORDER BY 1")]
    // Multi-column INTERSECT to check the all-columns-equal predicate.
    [InlineData("SELECT e.Country, e.City FROM Employees e INTERSECT SELECT c.Country, c.City FROM Customers c ORDER BY 1, 2")]
    // Scalar subquery whose relation isn't provably single-row -> cardinality guard
    // (aggregate + assert). The unique key keeps it to one row, so the assert passes.
    [InlineData("SELECT c.CustomerID, (SELECT o.OrderDate FROM Orders o WHERE o.OrderID = 10248) FROM Customers c ORDER BY c.CustomerID")]
    // Correlated scalar subquery that IS provably single-row (a scalar aggregate) ->
    // the guard is skipped, leaving a plain apply.
    [InlineData("SELECT c.CustomerID, (SELECT MAX(o.OrderID) FROM Orders o WHERE o.CustomerID = c.CustomerID) FROM Customers c ORDER BY c.CustomerID")]
    // FULL OUTER JOIN -> (left outer) UNION ALL (right-anti-semi, left null-padded).
    // City has unmatched rows on both sides, exercising both branches and the nulls.
    [InlineData("SELECT e.City, c.City FROM Employees e FULL JOIN Customers c ON e.City = c.City ORDER BY e.City, c.City")]
    // FULL OUTER where the condition rarely matches, stressing the null-padding.
    [InlineData("SELECT e.City, c.Country FROM Employees e FULL JOIN Customers c ON e.City = c.Country ORDER BY e.City, c.Country")]
    // FULL OUTER over a grouped/aggregated side -> the cloner must duplicate an
    // aggregate subtree (group keys + aggregate outputs) with fresh slots.
    [InlineData("SELECT g.City, c.City FROM (SELECT e.City, COUNT(*) AS n FROM Employees e GROUP BY e.City) g FULL JOIN Customers c ON g.City = c.City ORDER BY g.City, c.City")]
    // FULL OUTER over a computed side -> the cloner must duplicate a compute subtree.
    [InlineData("SELECT t.c, o.OrderID FROM (SELECT e.EmployeeID + 0 AS c FROM Employees e) t FULL JOIN Orders o ON t.c = o.EmployeeID ORDER BY t.c, o.OrderID")]
    // Hash match on a nullable key (ReportsTo) -> NULL keys never match, so the boss
    // row surfaces only via the left-outer path.
    [InlineData("SELECT e.EmployeeID, m.EmployeeID FROM Employees e LEFT JOIN Employees m ON e.ReportsTo = m.EmployeeID ORDER BY e.EmployeeID")]
    // Non-equi FULL OUTER -> no hash key, so the planner's expansion still runs.
    [InlineData("SELECT e.EmployeeID, t.n FROM Employees e FULL JOIN (SELECT e2.EmployeeID AS n FROM Employees e2 WHERE e2.EmployeeID <= 3) t ON e.EmployeeID < t.n ORDER BY e.EmployeeID, t.n")]
    // EXISTS in an inner join's ON, correlated to the join output -> hoisted into a
    // (probing) semi join above the join.
    [InlineData("SELECT o.OrderID, od.ProductID FROM Orders o INNER JOIN [Order Details] od ON o.OrderID = od.OrderID AND EXISTS (SELECT * FROM Customers c WHERE c.CustomerID = o.CustomerID AND c.Country = 'Germany') ORDER BY o.OrderID, od.ProductID")]
    // NOT EXISTS in an inner join's ON.
    [InlineData("SELECT o.OrderID, od.ProductID FROM Orders o INNER JOIN [Order Details] od ON o.OrderID = od.OrderID AND NOT EXISTS (SELECT * FROM Customers c WHERE c.CustomerID = o.CustomerID AND c.Country = 'Germany') ORDER BY o.OrderID, od.ProductID")]
    // Uncorrelated scalar aggregate subquery in an inner join's ON -> a cross-joined
    // single-row value the filter above the join tests.
    [InlineData("SELECT od.OrderID, od.ProductID FROM [Order Details] od INNER JOIN Products p ON od.ProductID = p.ProductID AND od.UnitPrice > (SELECT AVG(od2.UnitPrice) FROM [Order Details] od2) ORDER BY od.OrderID, od.ProductID")]
    // CASE passthru: a multi-row subquery in a THEN whose WHEN is always false must be
    // skipped -- otherwise its cardinality assert would fire even though the branch is
    // never taken.
    [InlineData("SELECT e.EmployeeID, CASE WHEN e.EmployeeID = 0 THEN (SELECT o.OrderID FROM Orders o) ELSE e.EmployeeID END FROM Employees e ORDER BY e.EmployeeID")]
    // CASE passthru on the ELSE branch (the WHEN is always true, so the ELSE subquery
    // is never evaluated).
    [InlineData("SELECT e.EmployeeID, CASE WHEN e.EmployeeID > 0 THEN 1 ELSE (SELECT o.OrderID FROM Orders o) END FROM Employees e ORDER BY e.EmployeeID")]
    // CASE passthru that is conditional: the correlated subquery runs only for the UK
    // rows the WHEN selects, and the values still come out right.
    [InlineData("SELECT e.EmployeeID, CASE WHEN e.Country = 'UK' THEN (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) ELSE -1 END FROM Employees e ORDER BY e.EmployeeID")]
    // EXISTS in a LEFT join's ON, correlated to the right side -> the Apply is pushed onto
    // the join's right input (not hoisted above), so the outer-join null-padding is preserved.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND EXISTS (SELECT * FROM Customers c WHERE c.CustomerID = o.CustomerID AND c.Country = 'Germany') ORDER BY e.EmployeeID, o.OrderID")]
    // NOT EXISTS in a LEFT join's ON, correlated to the right side.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND NOT EXISTS (SELECT * FROM Customers c WHERE c.CustomerID = o.CustomerID AND c.Country = 'Germany') ORDER BY e.EmployeeID, o.OrderID")]
    // Uncorrelated scalar aggregate subquery in a LEFT join's ON -> a single-row value the
    // join condition tests; pushed onto the right, the unmatched left rows still null-pad.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND o.Freight > (SELECT AVG(o2.Freight) FROM Orders o2) ORDER BY e.EmployeeID, o.OrderID")]
    // Correlated scalar subquery in a LEFT join's ON, correlated to the right side.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND o.Freight > (SELECT MAX(o2.Freight) FROM Orders o2 WHERE o2.CustomerID = o.CustomerID) ORDER BY e.EmployeeID, o.OrderID")]
    // EXISTS in a LEFT join's ON correlated to the join's LEFT (e.EmployeeID) -> the whole join
    // is lowered as a LeftOuter dependent join (the right-side push can't reach the left). The
    // left rows whose subquery is empty must still null-pad.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND EXISTS (SELECT * FROM Orders o2 WHERE o2.EmployeeID = e.EmployeeID) ORDER BY e.EmployeeID, o.OrderID")]
    // Left-correlated subquery in a LEFT join's ON that ALSO references the right (both sides):
    // decorrelation moves the predicate onto the inner semi join, whose condition then
    // references the outer e.EmployeeID -- a correlation on the join itself, now supported.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND EXISTS (SELECT * FROM Orders o2 WHERE o2.EmployeeID = e.EmployeeID AND o2.OrderID < o.OrderID) ORDER BY e.EmployeeID, o.OrderID")]
    // NOT EXISTS in a LEFT join's ON correlated to the left.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND NOT EXISTS (SELECT * FROM Customers c WHERE c.Country = e.Country) ORDER BY e.EmployeeID, o.OrderID")]
    // Left-correlated scalar aggregate subquery in a LEFT join's ON.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND o.Freight > (SELECT MAX(o2.Freight) FROM Orders o2 WHERE o2.EmployeeID = e.EmployeeID) ORDER BY e.EmployeeID, o.OrderID")]
    // FULL OUTER JOIN whose ON has a subquery correlated to the left -> no single Apply form,
    // so it is expanded into (LEFT OUTER) UNION ALL (right-anti-semi), each branch a dependent
    // join. The subquery is selective (only employees with a high-freight order satisfy it), so
    // both the null-padded-right and null-padded-left sides are exercised with the correlation.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e FULL JOIN Orders o ON e.EmployeeID = o.EmployeeID AND EXISTS (SELECT * FROM Orders o2 WHERE o2.EmployeeID = e.EmployeeID AND o2.Freight > 500) ORDER BY e.EmployeeID, o.OrderID")]
    public void NewPipeline_ProducesSameRows_AsUnoptimized(string text)
    {
        var expected = RunUnoptimized(text);
        var actual = RunNewPipeline(text);

        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
            Assert.Equal(expected[i], actual[i]);
    }

    [Theory]
    // Correlated aggregate subqueries that now decorrelate. Each is checked against the
    // nested-loops reference (RunUnoptimized), so the decorrelated plan must agree row for
    // row. The Customers cases are the COUNT-bug trigger: many customers have no orders, so
    // the count must come out 0 (not NULL, and not 1 from a null-padded row).
    [InlineData("SELECT e.EmployeeID, (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e ORDER BY e.EmployeeID")]
    [InlineData("SELECT c.CustomerID, (SELECT COUNT(*) FROM Orders o WHERE o.CustomerID = c.CustomerID) FROM Customers c ORDER BY c.CustomerID")]
    [InlineData("SELECT e.EmployeeID, (SELECT MAX(o.OrderID) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e ORDER BY e.EmployeeID")]
    [InlineData("SELECT e.EmployeeID, (SELECT SUM(o.Freight) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e ORDER BY e.EmployeeID")]
    // A correlation key with duplicates in the outer (City) must keep the right
    // cardinality through the domain join-back.
    [InlineData("SELECT e.City, (SELECT COUNT(*) FROM Orders o WHERE o.ShipCity = e.City) FROM Employees e ORDER BY e.City, 2")]
    // Correlation on a NULL-bearing key: rows whose key is NULL must group with the NULL
    // domain key (null-safe join-back), not vanish.
    [InlineData("SELECT e.EmployeeID, (SELECT COUNT(*) FROM Employees m WHERE m.ReportsTo = e.ReportsTo) FROM Employees e ORDER BY e.EmployeeID")]
    public void Decorrelation_MatchesNestedLoops_ForCorrelatedAggregate(string text)
    {
        var decorrelated = RunNewPipeline(text);
        var nestedLoops = RunUnoptimized(text);

        Assert.Equal(nestedLoops.Count, decorrelated.Count);
        for (var i = 0; i < nestedLoops.Count; i++)
            Assert.Equal(nestedLoops[i], decorrelated[i]);
    }

    [Theory]
    // Null-rejection outer-join removal: WHERE rejects the right's NULLs, so the LEFT
    // JOIN becomes an INNER JOIN -- the rows must match the un-simplified plan.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID WHERE o.OrderID < 10260 ORDER BY e.EmployeeID, o.OrderID")]
    // A non-rejecting WHERE (IS NULL keeps the padded rows) must NOT be tightened.
    [InlineData("SELECT c.CustomerID FROM Customers c LEFT JOIN Orders o ON c.CustomerID = o.CustomerID WHERE o.OrderID IS NULL ORDER BY c.CustomerID")]
    // Rejection harvested from an inner join's own condition frees a deeper LEFT JOIN.
    [InlineData("SELECT e.EmployeeID, od.ProductID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID INNER JOIN [Order Details] od ON od.OrderID = o.OrderID WHERE od.ProductID < 20 ORDER BY e.EmployeeID, od.ProductID")]
    // FULL OUTER with only the right rejected -> swapped to a LEFT OUTER (Customers left).
    [InlineData("SELECT e.City, c.City FROM Employees e FULL JOIN Customers c ON e.City = c.City WHERE c.City = 'London' ORDER BY e.City, c.City")]
    // Computed equi-join key: ON e.EmployeeID + 0 = o.EmployeeID becomes slot = slot and
    // hash-matches; rows must match the nested-loops plan.
    [InlineData("SELECT e.EmployeeID, o.OrderID FROM Employees e INNER JOIN Orders o ON e.EmployeeID + 0 = o.EmployeeID WHERE o.OrderID < 10260 ORDER BY e.EmployeeID, o.OrderID")]
    // Left-outer pull-up: Orders (preserved side) joins Employees in the inner region and
    // the LEFT JOIN to [Order Details] is re-applied on top; rows must match the
    // un-reordered plan, including the null-padded rows for orders with no details.
    [InlineData("SELECT e.EmployeeID, od.ProductID FROM Employees e, (Orders o LEFT JOIN [Order Details] od ON o.OrderID = od.OrderID) WHERE e.EmployeeID = o.EmployeeID AND o.OrderID < 10250 ORDER BY e.EmployeeID, od.ProductID")]
    public void Optimization_MatchesUnoptimized(string text)
    {
        var optimized = RunNewPipeline(text);
        var reference = RunUnoptimized(text);

        Assert.Equal(reference.Count, optimized.Count);
        for (var i = 0; i < reference.Count; i++)
            Assert.Equal(reference[i], optimized[i]);
    }

    [Fact]
    public void ExecutablePlan_IsReusable_AcrossCreateIterator()
    {
        // The compiled delegates take the row buffer as a parameter, so a single
        // emitted plan can produce independent iterators -- each run yields the
        // same rows.
        var text = "SELECT e.FirstName, e.EmployeeID + 1 FROM Employees e WHERE e.City = 'London'";
        var plan = Emitter.Emit(Planner.Plan(LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text)), NorthwindCatalog.Instance)));

        var first = Drain(plan);
        var second = Drain(plan);

        Assert.NotEmpty(first);
        Assert.Equal(first.Count, second.Count);
        for (var i = 0; i < first.Count; i++)
            Assert.Equal(first[i], second[i]);
    }

    [Fact]
    public void NewPipeline_ThrowsForMultiRowScalarSubquery()
    {
        // A scalar subquery that returns more than one row is a runtime error: the
        // cardinality guard's assert fires, optimized or not.
        var text = "SELECT (SELECT e.City FROM Employees e) FROM Customers c";

        Assert.Throws<InvalidOperationException>(() => RunUnoptimized(text));
        Assert.Throws<InvalidOperationException>(() => RunNewPipeline(text));
    }

    private static List<object[]> RunNewPipeline(string text)
    {
        var physicalQuery = Planner.Plan(LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text)), NorthwindCatalog.Instance));
        var plan = Emitter.Emit(physicalQuery);
        return Drain(plan);
    }

    // The pipeline with no logical optimization: a correlated subquery stays an Apply and
    // runs as nested loops. That is the semantically-correct reference for decorrelation,
    // and -- unlike the Query API, which now also decorrelates -- an independent oracle.
    private static List<object[]> RunUnoptimized(string text)
    {
        var physicalQuery = Planner.Plan(Algebrizer.Algebrize(Bind(text)));
        var plan = Emitter.Emit(physicalQuery);
        return Drain(plan);
    }

    private static List<object[]> Drain(NQuery.CodeAnalysis.Emit.ExecutablePlan plan)
    {
        var iterator = plan.CreateIterator();
        using (iterator)
        {
            iterator.Open();

            // Resolve each output column to its row-buffer address (the reader does the same).
            var allocation = new NQuery.CodeAnalysis.Iterators.RowBufferAllocation(null, iterator.RowBuffer, plan.OutputValueSlots);
            var entries = plan.OutputValueSlots.Select(s => allocation[s]).ToArray();

            var rows = new List<object[]>();
            while (iterator.Read())
            {
                var row = new object[entries.Length];
                for (var i = 0; i < entries.Length; i++)
                    row[i] = entries[i].GetValue()!;
                rows.Add(row);
            }

            return rows;
        }
    }

    private static BoundQuery Bind(string text)
    {
        var syntaxTree = SyntaxTree.ParseQuery(text);
        var bindingResult = Binder.Bind(syntaxTree.Root, NorthwindCatalog.Instance);
        Assert.Empty(syntaxTree.GetDiagnostics().Concat(bindingResult.Diagnostics));
        return (BoundQuery)bindingResult.BoundRoot;
    }
}
