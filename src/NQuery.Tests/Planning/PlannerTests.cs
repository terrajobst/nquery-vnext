using NQuery.Refactor.Algebra;
using NQuery.Refactor.Binding;
using NQuery.Refactor.Optimization;
using NQuery.Planning;

namespace NQuery.Tests.Planning
{
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
            var logicalQuery = LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text)));
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

        private static PhysicalOperator Plan(string text)
        {
            return Planner.Plan(LogicalOptimizer.Optimize(Algebrizer.Algebrize(Bind(text)))).Root;
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
