using NQuery.Algebra;
using NQuery.Binding;
using NQuery.AlgebraOptimization;

namespace NQuery.Tests.AlgebraOptimization
{
    public class LogicalOptimizerTests
    {
        [Theory]
        [InlineData("SELECT e.FirstName FROM Employees e")]
        [InlineData("SELECT e.FirstName FROM Employees e INNER JOIN Orders o ON e.EmployeeID = o.EmployeeID WHERE e.City = 'London'")]
        [InlineData("SELECT (SELECT COUNT(*) FROM Orders o) FROM Employees e")]
        [InlineData("SELECT e.FirstName FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)")]
        [InlineData("SELECT e.FirstName FROM Employees e WHERE NOT EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)")]
        [InlineData("SELECT o.OrderID FROM Orders o, [Order Details] od WHERE o.OrderID = od.OrderID")]
        [InlineData("SELECT e.LastName FROM Employees e, Orders o, [Order Details] od WHERE e.EmployeeID = o.EmployeeID AND o.OrderID = od.OrderID")]
        public void Optimizer_PreservesOutput(string text)
        {
            var logicalQuery = Algebrizer.Algebrize(Bind(text));
            var optimized = LogicalOptimizer.Optimize(logicalQuery);

            Assert.Equal(logicalQuery.Root.OutputValueSlots, optimized.Root.OutputValueSlots);
            Assert.Equal(logicalQuery.OutputColumns, optimized.OutputColumns);
        }

        [Fact]
        public void ApplyPushdown_Decorrelates_Exists_IntoSemiJoin()
        {
            var text = "SELECT e.FirstName FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)";
            var optimized = Optimize(text);

            Assert.DoesNotContain(optimized.DescendantsAndSelf(), o => o is LogicalApply);

            var join = optimized.DescendantsAndSelf().OfType<LogicalJoin>().Single();
            Assert.Equal(LogicalJoinKind.LeftSemi, join.JoinKind);
            Assert.NotEmpty(join.Conditions);   // the former correlation predicate
            Assert.NotNull(join.Probe);       // the EXISTS probe survives onto the join
        }

        [Fact]
        public void ApplyPushdown_Decorrelates_UncorrelatedScalar_IntoOuterJoin()
        {
            var text = "SELECT (SELECT COUNT(*) FROM Orders o) FROM Employees e";
            var optimized = Optimize(text);

            Assert.DoesNotContain(optimized.DescendantsAndSelf(), o => o is LogicalApply);

            var join = optimized.DescendantsAndSelf().OfType<LogicalJoin>().Single();
            Assert.Equal(LogicalJoinKind.LeftOuter, join.JoinKind);
            Assert.Empty(join.Conditions);      // uncorrelated -> no join predicate
        }

        [Fact]
        public void SelectionPushdown_PushesPredicate_ToTableScan()
        {
            var text = "SELECT e.FirstName FROM Employees e INNER JOIN Orders o ON e.EmployeeID = o.EmployeeID WHERE e.City = 'London'";
            var optimized = Optimize(text);

            // The WHERE predicate references only Employees, so it should end up as a
            // filter sitting directly on the Employees table scan, below the join.
            var pushed = optimized.DescendantsAndSelf()
                                  .OfType<LogicalFilter>()
                                  .Any(f => f.Input is LogicalTableScan);
            Assert.True(pushed);

            // ...and there should no longer be a filter sitting on top of the join.
            var aboveJoin = optimized.DescendantsAndSelf()
                                     .OfType<LogicalFilter>()
                                     .Any(f => f.Input is LogicalJoin);
            Assert.False(aboveJoin);
        }

        [Fact]
        public void SelectionPushdown_SplitsConjuncts_AcrossJoinSides()
        {
            // e.City references the left, o.Freight the right: the AND should split,
            // each conjunct landing on its own table scan with no filter left above
            // the join.
            var text = "SELECT e.FirstName FROM Employees e INNER JOIN Orders o ON e.EmployeeID = o.EmployeeID WHERE e.City = 'London' AND o.Freight > 100";
            var optimized = Optimize(text);

            var filtersOnScans = optimized.DescendantsAndSelf()
                                          .OfType<LogicalFilter>()
                                          .Count(f => f.Input is LogicalTableScan);
            Assert.Equal(2, filtersOnScans);

            Assert.DoesNotContain(optimized.DescendantsAndSelf(), o => o is LogicalFilter { Input: LogicalJoin });
        }

        [Fact]
        public void JoinOrderer_TurnsCrossProductPlusFilter_IntoEquiJoin()
        {
            var text = "SELECT o.OrderID FROM Orders o, [Order Details] od WHERE o.OrderID = od.OrderID";
            var optimized = Optimize(text);

            // The comma-join + WHERE became a single inner join with the predicate as
            // its condition: no cross product (empty-condition join) and no filter.
            var join = optimized.DescendantsAndSelf().OfType<LogicalJoin>().Single();
            Assert.Equal(LogicalJoinKind.Inner, join.JoinKind);
            Assert.NotEmpty(join.Conditions);
            Assert.DoesNotContain(optimized.DescendantsAndSelf(), o => o is LogicalFilter);
        }

        [Fact]
        public void JoinOrderer_BuildsLeftDeepTree_WithConditionsPlaced()
        {
            var text = "SELECT e.LastName FROM Employees e, Orders o, [Order Details] od " +
                       "WHERE e.EmployeeID = o.EmployeeID AND o.OrderID = od.OrderID";
            var optimized = Optimize(text);

            var joins = optimized.DescendantsAndSelf().OfType<LogicalJoin>().ToList();

            Assert.Equal(2, joins.Count);
            Assert.All(joins, j => Assert.Equal(LogicalJoinKind.Inner, j.JoinKind));
            // Left-deep: no join is the right input of another join.
            Assert.All(joins, j => Assert.IsNotType<LogicalJoin>(j.Right));
            // Every join carries a predicate -- no cartesian products were introduced.
            Assert.All(joins, j => Assert.NotEmpty(j.Conditions));
            Assert.DoesNotContain(optimized.DescendantsAndSelf(), o => o is LogicalFilter);
        }

        private static LogicalOperator Optimize(string text)
        {
            return LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text))).Root;
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
