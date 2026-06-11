using NQuery.Algebra;
using NQuery.Binding;
using NQuery.Emit;
using NQuery.EmittedIterators;
using NQuery.LogicalOptimization;
using NQuery.Planning;

namespace NQuery.Tests.Iterators
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
        public void NewPipeline_ProducesSameRows_AsExistingEngine(string text)
        {
            var expected = RunExistingEngine(text);
            var actual = RunNewPipeline(text);

            Assert.Equal(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
                Assert.Equal(expected[i], actual[i]);
        }

        [Fact]
        public void ExecutablePlan_IsReusable_AcrossCreateIterator()
        {
            // The compiled delegates take the row buffer as a parameter, so a single
            // emitted plan can produce independent iterators -- each run yields the
            // same rows.
            var text = "SELECT e.FirstName, e.EmployeeID + 1 FROM Employees e WHERE e.City = 'London'";
            var plan = Emitter.Emit(Planner.Plan(LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text)))));

            var first = Drain(plan.CreateIterator());
            var second = Drain(plan.CreateIterator());

            Assert.NotEmpty(first);
            Assert.Equal(first.Count, second.Count);
            for (var i = 0; i < first.Count; i++)
                Assert.Equal(first[i], second[i]);
        }

        private static List<object[]> RunNewPipeline(string text)
        {
            var physicalQuery = Planner.Plan(LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text))));
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
