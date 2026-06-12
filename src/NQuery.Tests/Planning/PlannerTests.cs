using NQuery.Algebra;
using NQuery.Binding;
using NQuery.AlgebraOptimization;
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
        public void Planner_BuildsNestedLoops_ForEquiJoin()
        {
            var text = "SELECT o.OrderID FROM Orders o INNER JOIN [Order Details] od ON o.OrderID = od.OrderID";
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
