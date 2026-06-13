using System.Collections.Immutable;

using NQuery.Refactor.Algebra;
using NQuery.Refactor.Binding;
using NQuery.Refactor.Optimization;

namespace NQuery.Tests.Refactor.Optimization
{
    public class LogicalOptimizerTests
    {
        [Theory]
        [InlineData("SELECT e.FirstName FROM Employees e")]
        [InlineData("SELECT e.FirstName FROM Employees e INNER JOIN Orders o ON e.EmployeeID = o.EmployeeID WHERE e.City = 'London'")]
        [InlineData("SELECT (SELECT COUNT(*) FROM Orders o) FROM Employees e")]
        [InlineData("SELECT (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e")]
        [InlineData("SELECT (SELECT MAX(o.OrderID) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e")]
        [InlineData("SELECT e.FirstName FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)")]
        [InlineData("SELECT e.FirstName FROM Employees e WHERE NOT EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)")]
        [InlineData("SELECT o.OrderID FROM Orders o, [Order Details] od WHERE o.OrderID = od.OrderID")]
        [InlineData("SELECT e.LastName FROM Employees e, Orders o, [Order Details] od WHERE e.EmployeeID = o.EmployeeID AND o.OrderID = od.OrderID")]
        public void Optimizer_PreservesOutput(string text)
        {
            var logicalQuery = Algebrizer.Algebrize(Bind(text));
            var optimized = LogicalOptimizer.Optimize(logicalQuery, NorthwindDataContext.Instance);

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
        public void ApplyPushdown_Decorrelates_CorrelatedAggregate_IntoJoinAndAggregate()
        {
            // The correlated COUNT subquery should decorrelate: no apply survives, and the
            // tree gains a join (the domain join-back) and an aggregate (the per-key counts).
            var text = "SELECT (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e";
            var optimized = Optimize(text);

            Assert.DoesNotContain(optimized.DescendantsAndSelf(), o => o is LogicalApply);
            Assert.Contains(optimized.DescendantsAndSelf(), o => o is LogicalJoin);
            Assert.Contains(optimized.DescendantsAndSelf(), o => o is LogicalAggregate);
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

        [Fact]
        public void OuterJoinRemover_RewritesLeftOuterAsInner_WhenPredicateRejectsNulls()
        {
            // WHERE o.OrderID = 10248 can never hold for a null-padded Orders row, so the
            // LEFT JOIN tightens to an INNER JOIN.
            var text = "SELECT e.FirstName FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID WHERE o.OrderID = 10248";
            var optimized = Optimize(text);

            Assert.DoesNotContain(optimized.DescendantsAndSelf(), o => o is LogicalJoin { JoinKind: LogicalJoinKind.LeftOuter });
            Assert.Contains(optimized.DescendantsAndSelf(), o => o is LogicalJoin { JoinKind: LogicalJoinKind.Inner });
        }

        [Fact]
        public void OuterJoinRemover_KeepsLeftOuter_WhenPredicateDoesNotRejectNulls()
        {
            // WHERE o.OrderID IS NULL keeps exactly the null-padded rows, so the LEFT JOIN
            // must survive.
            var text = "SELECT e.FirstName FROM Employees e LEFT JOIN Orders o ON e.EmployeeID = o.EmployeeID WHERE o.OrderID IS NULL";
            var optimized = Optimize(text);

            Assert.Contains(optimized.DescendantsAndSelf(), o => o is LogicalJoin { JoinKind: LogicalJoinKind.LeftOuter });
        }

        [Fact]
        public void OuterJoinRemover_SwapsFullOuterToLeftOuter_WhenOnlyRightIsRejected()
        {
            // WHERE c.City = 'London' rejects the right's NULLs, so the FULL OUTER keeps only
            // rows where Customers is present -- a RIGHT OUTER, swapped to a LEFT OUTER with
            // Customers on the left.
            var text = "SELECT e.City, c.City FROM Employees e FULL JOIN Customers c ON e.City = c.City WHERE c.City = 'London'";
            var optimized = Optimize(text);

            Assert.DoesNotContain(optimized.DescendantsAndSelf(), o => o is LogicalJoin { JoinKind: LogicalJoinKind.FullOuter });

            var join = optimized.DescendantsAndSelf().OfType<LogicalJoin>().Single(j => j.JoinKind == LogicalJoinKind.LeftOuter);
            var leftScan = Assert.IsType<LogicalTableScan>(join.Left);
            Assert.Equal("Customers", leftScan.TableInstance.Table.Name);
        }

        [Fact]
        public void OuterJoinRemover_RemovesConstantSide_OfInnerJoin()
        {
            // An inner join against the single-row constant relation collapses to the other
            // side. The algebrizer rarely emits a bare constant join, so build it directly.
            var employees = Algebrizer.Algebrize(Bind("SELECT e.City FROM Employees e")).Root;
            var join = new LogicalJoin(LogicalJoinKind.Inner, new LogicalConstant(), employees,
                                       ImmutableArray<LogicalExpression>.Empty, probe: null, passthruPredicate: null);

            var result = new OuterJoinRemover().RewriteRelation(join);

            // No condition, so the constant side drops with nothing left to filter.
            Assert.Same(employees, result);
        }

        [Fact]
        public void Optimizer_LeavesComputedJoinKeyInline()
        {
            // ON e.EmployeeID + 0 = o.EmployeeID stays a computed condition in the logical
            // tree -- materializing it into a hash key is the planner's job (see
            // PlannerTests), so the nested-loops fallback keeps the predicate inline.
            var text = "SELECT e.FirstName FROM Employees e INNER JOIN Orders o ON e.EmployeeID + 0 = o.EmployeeID";
            var optimized = Optimize(text);

            var join = optimized.DescendantsAndSelf().OfType<LogicalJoin>().Single();
            var equal = Assert.IsType<LogicalBinaryExpression>(join.Conditions.Single());
            Assert.IsType<LogicalBinaryExpression>(equal.Left);   // e.EmployeeID + 0, not hoisted
            Assert.DoesNotContain(optimized.DescendantsAndSelf(), o => o is LogicalCompute);
        }

        [Fact]
        public void JoinOrderer_PullsUpLeftOuterJoin_WhenNoPredicateReferencesNullSuppliedSide()
        {
            // od is referenced by no region predicate, so the preserved side (Orders) is
            // pulled into the inner region with Employees and the LEFT JOIN to od re-applied
            // on top -- the topmost join becomes the left outer.
            var text = "SELECT e.EmployeeID FROM Employees e, (Orders o LEFT JOIN [Order Details] od ON o.OrderID = od.OrderID) WHERE e.EmployeeID = o.EmployeeID";
            var optimized = Optimize(text);

            var topmostJoin = optimized.DescendantsAndSelf().OfType<LogicalJoin>().First();
            Assert.Equal(LogicalJoinKind.LeftOuter, topmostJoin.JoinKind);

            // ...and below it, an inner join between Employees and Orders carrying the predicate.
            var inner = optimized.DescendantsAndSelf().OfType<LogicalJoin>().Single(j => j.JoinKind == LogicalJoinKind.Inner);
            Assert.NotEmpty(inner.Conditions);
        }

        [Fact]
        public void JoinOrderer_KeepsLeftOuterJoin_WhenPredicateReferencesNullSuppliedSide()
        {
            // od.ProductID IS NULL references the null-supplied side (and doesn't reject its
            // NULLs, so OuterJoinRemover leaves it), which makes the outer join un-poolable:
            // it stays an opaque region leaf under the inner join.
            var text = "SELECT e.EmployeeID FROM Employees e, (Orders o LEFT JOIN [Order Details] od ON o.OrderID = od.OrderID) WHERE e.EmployeeID = o.EmployeeID AND od.ProductID IS NULL";
            var optimized = Optimize(text);

            var topmostJoin = optimized.DescendantsAndSelf().OfType<LogicalJoin>().First();
            Assert.Equal(LogicalJoinKind.Inner, topmostJoin.JoinKind);
            Assert.Contains(optimized.DescendantsAndSelf(), o => o is LogicalJoin { JoinKind: LogicalJoinKind.LeftOuter });
        }

        private static LogicalOperator Optimize(string text)
        {
            return LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text)), NorthwindDataContext.Instance).Root;
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
