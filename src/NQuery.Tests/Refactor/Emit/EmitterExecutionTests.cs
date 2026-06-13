using NQuery.Refactor.Algebra;
using NQuery.Refactor.Binding;
using NQuery.Refactor.Emit;
using NQuery.Refactor.Iterators;
using NQuery.Refactor.Optimization;
using NQuery.Planning;

namespace NQuery.Tests.Refactor.Emit
{
    // End-to-end execution through the new pipeline
    // (Bind -> Algebrize -> Optimize -> Plan -> Emit -> CreateIterator), checked
    // differentially against the existing engine. Covers the operators the Emitter
    // wires so far: scan, filter, compute, project, sort, top, and nested-loops joins
    // (inner, cross, left outer, and probing semi via EXISTS / NOT EXISTS). Queries
    // carry an ORDER BY so the row order is deterministic across both engines.
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
        public void NewPipeline_ProducesSameRows_AsExistingEngine(string text)
        {
            var expected = RunExistingEngine(text);
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

        [Fact]
        public void ExecutablePlan_IsReusable_AcrossCreateIterator()
        {
            // The compiled delegates take the row buffer as a parameter, so a single
            // emitted plan can produce independent iterators -- each run yields the
            // same rows.
            var text = "SELECT e.FirstName, e.EmployeeID + 1 FROM Employees e WHERE e.City = 'London'";
            var plan = Emitter.Emit(Planner.Plan(LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text)), NorthwindDataContext.Instance)));

            var first = Drain(plan.CreateIterator());
            var second = Drain(plan.CreateIterator());

            Assert.NotEmpty(first);
            Assert.Equal(first.Count, second.Count);
            for (var i = 0; i < first.Count; i++)
                Assert.Equal(first[i], second[i]);
        }

        [Fact]
        public void NewPipeline_DoesNotSupport_SubqueryInOuterJoinCondition()
        {
            // Hoisting an ON conjunct above the join changes outer-join semantics, so a
            // subquery in a non-inner join's ON isn't lowered (yet).
            var text = "SELECT e.EmployeeID, o.OrderID FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID AND EXISTS (SELECT * FROM Customers c WHERE c.CustomerID = o.CustomerID)";

            Assert.Throws<NotSupportedException>(() => RunNewPipeline(text));
        }

        [Fact]
        public void NewPipeline_ThrowsForMultiRowScalarSubquery()
        {
            // A scalar subquery that returns more than one row is a runtime error in
            // both engines (the cardinality guard's assert fires).
            var text = "SELECT (SELECT e.City FROM Employees e) FROM Customers c";

            Assert.Throws<InvalidOperationException>(() => RunExistingEngine(text));
            Assert.Throws<InvalidOperationException>(() => RunNewPipeline(text));
        }

        private static List<object[]> RunNewPipeline(string text)
        {
            var physicalQuery = Planner.Plan(LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text)), NorthwindDataContext.Instance));
            var plan = Emitter.Emit(physicalQuery);
            return Drain(plan.CreateIterator());
        }

        // The pipeline with no logical optimization: a correlated subquery stays an Apply and
        // runs as nested loops. That is the semantically-correct reference for decorrelation,
        // and -- unlike the Query API, which now also decorrelates -- an independent oracle.
        private static List<object[]> RunUnoptimized(string text)
        {
            var physicalQuery = Planner.Plan(Algebrizer.Algebrize(Bind(text)));
            var plan = Emitter.Emit(physicalQuery);
            return Drain(plan.CreateIterator());
        }

        private static List<object[]> Drain(Iterator iterator)
        {
            using (iterator)
            {
                iterator.Open();

                var rows = new List<object[]>();
                while (iterator.Read())
                {
                    var rowBuffer = iterator.RowBuffer;
                    var row = new object[rowBuffer.Count];
                    for (var i = 0; i < row.Length; i++)
                        row[i] = rowBuffer[i];
                    rows.Add(row);
                }

                return rows;
            }
        }

        private static List<object[]> RunExistingEngine(string text)
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

        private static BoundQuery Bind(string text)
        {
            var syntaxTree = SyntaxTree.ParseQuery(text);
            var bindingResult = Binder.Bind(syntaxTree.Root, NorthwindDataContext.Instance);
            Assert.Empty(syntaxTree.GetDiagnostics().Concat(bindingResult.Diagnostics));
            return (BoundQuery)bindingResult.BoundRoot;
        }
    }
}
