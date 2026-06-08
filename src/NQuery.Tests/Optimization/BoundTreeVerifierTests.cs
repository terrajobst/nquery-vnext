using NQuery.Binding;
using NQuery.Optimization;

namespace NQuery.Tests.Optimization
{
    public class BoundTreeVerifierTests
    {
        // A representative corpus that exercises the slot-bearing relations:
        // joins, outer joins, grouping, distinct, ordering, top, set operations,
        // CTEs, derived tables, correlated subselects, EXISTS and IN.
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
        public void BoundTreeVerifier_Accepts_EveryOptimizationStage(string text)
        {
            var boundQuery = Bind(text);

            // The freshly-bound tree must be valid...
            BoundTreeVerifier.Verify(boundQuery);

            // ...and must remain valid after each individual optimization pass.
            // If a pass drops or aliases a value slot, the verifier fails here,
            // pinpointing the pass that broke the tree.
            var relation = boundQuery.Relation;
            foreach (var rewriter in Optimizer.GetOptimizationSteps())
            {
                relation = rewriter.RewriteRelation(relation);
                try
                {
                    BoundTreeVerifier.Verify(relation);
                }
                catch (BoundTreeVerificationException e)
                {
                    Assert.Fail($"{rewriter.GetType().Name} produced an invalid tree for `{text}`:{Environment.NewLine}{e.Message}");
                }
            }
        }

        [Fact]
        public void BoundTreeVerifier_Rejects_DanglingSlotReference()
        {
            // A filter whose condition reads a slot that nothing below it defines.
            var factory = new ValueSlotFactory();
            var orphan = factory.CreateTemporary(typeof(bool));
            var condition = new BoundValueSlotExpression(orphan);
            var filter = new BoundFilterRelation(new BoundConstantRelation(), condition);

            var exception = Assert.Throws<BoundTreeVerificationException>(() => BoundTreeVerifier.Verify(filter));
            Assert.Contains(orphan.Name, exception.Message);
        }

        [Fact]
        public void BoundTreeVerifier_Rejects_ScopeEscapingReference()
        {
            // Correlation only flows left-to-right across a join: the left side may
            // not see the right side. A left-side filter referencing a right-side
            // slot is therefore a scope escape.
            var leftTable = TableRelation("Employees", out _);
            var rightTable = TableRelation("Customers", out var rightSlots);

            var condition = new BoundValueSlotExpression(rightSlots[0]);
            var badLeft = new BoundFilterRelation(leftTable, condition);
            var join = new BoundJoinRelation(BoundJoinType.Inner, badLeft, rightTable, null, null, null);

            var exception = Assert.Throws<BoundTreeVerificationException>(() => BoundTreeVerifier.Verify(join));
            Assert.Contains(rightSlots[0].Name, exception.Message);
        }

        [Fact]
        public void BoundTreeVerifier_Allows_CorrelatedReferenceFromJoinRightToLeft()
        {
            // The same shape as above, but the slot legitimately comes from the
            // left side of the *enclosing* join, which the right side may see.
            var leftTable = TableRelation("Employees", out var leftSlots);
            var rightTable = TableRelation("Customers", out _);

            var condition = new BoundValueSlotExpression(leftSlots[0]);
            var correlatedRight = new BoundFilterRelation(rightTable, condition);
            var join = new BoundJoinRelation(BoundJoinType.Inner, leftTable, correlatedRight, null, null, null);

            BoundTreeVerifier.Verify(join); // does not throw
        }

        private static BoundTableRelation TableRelation(string name, out ValueSlot[] slots)
        {
            var table = NorthwindDataContext.Instance.Tables.Single(t => t.Name == name);
            var binding = Bind($"SELECT * FROM {name}");
            var relation = FindTableRelation(binding.Relation, name);
            slots = relation.GetDefinedValues().ToArray();
            return relation;
        }

        private static BoundTableRelation FindTableRelation(BoundRelation relation, string name)
        {
            var finder = new TableRelationFinder(name);
            finder.VisitRelation(relation);
            return finder.Result;
        }

        private static BoundQuery Bind(string text)
        {
            var syntaxTree = SyntaxTree.ParseQuery(text);
            var bindingResult = Binder.Bind(syntaxTree.Root, NorthwindDataContext.Instance);
            Assert.Empty(syntaxTree.GetDiagnostics().Concat(bindingResult.Diagnostics));
            return (BoundQuery)bindingResult.BoundRoot;
        }

        private sealed class TableRelationFinder : BoundTreeWalker
        {
            private readonly string _name;

            public TableRelationFinder(string name)
            {
                _name = name;
            }

            public BoundTableRelation Result { get; private set; }

            protected override void VisitTableRelation(BoundTableRelation node)
            {
                if (Result is null && string.Equals(node.TableInstance.Table.Name, _name, StringComparison.OrdinalIgnoreCase))
                    Result = node;
            }
        }
    }
}
