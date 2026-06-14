namespace NQuery.Tests;

public class ShowPlanTests
{
    // The algebrizer corpus -- one query per relation/expression kind the new
    // pipeline produces -- so every Build* path in both show-plan builders is exercised.
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
    [InlineData("SELECT e.City, c.City FROM Employees e FULL JOIN Customers c ON e.City = c.City")]
    [InlineData("SELECT e.LastName, c.City FROM Employees e, Customers c")]
    [InlineData("SELECT e.City FROM Employees e UNION SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e UNION ALL SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e EXCEPT SELECT c.City FROM Customers c")]
    [InlineData("SELECT e.City FROM Employees e INTERSECT SELECT c.City FROM Customers c")]
    [InlineData("SELECT d.City FROM (SELECT e.City FROM Employees e) AS d")]
    [InlineData("WITH X AS (SELECT e.City FROM Employees e) SELECT x.City FROM X x")]
    [InlineData("SELECT (SELECT COUNT(*) FROM Orders o WHERE o.EmployeeID = e.EmployeeID) FROM Employees e")]
    [InlineData("SELECT e.FirstName FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)")]
    [InlineData("SELECT e.FirstName FROM Employees e WHERE e.EmployeeID IN (SELECT o.EmployeeID FROM Orders o)")]
    [InlineData("SELECT CASE WHEN e.City = 'London' THEN 1 ELSE 0 END FROM Employees e")]
    [InlineData("SELECT CAST(e.EmployeeID AS FLOAT) FROM Employees e")]
    [InlineData("SELECT -e.EmployeeID, e.EmployeeID + 1 FROM Employees e")]
    [InlineData("SELECT e.FirstName FROM Employees e WHERE e.ReportsTo IS NULL")]
    [InlineData("SELECT COALESCE(e.Region, 'n/a') FROM Employees e")]
    [InlineData("SELECT e.FirstName.Length FROM Employees e")]
    [InlineData("SELECT e.FirstName.Substring(0, 1) FROM Employees e")]
    public void ShowPlan_Steps_StartAtUnoptimizedLogical_AndEndAtPhysical(string text)
    {
        var steps = Compile(text).GetShowPlanSteps().ToList();

        Assert.NotEmpty(steps);
        Assert.Equal("Unoptimized", steps[0].Name);
        Assert.Equal("Physical", steps[^1].Name);
        Assert.Contains(steps, s => s.Name == "Optimized");

        // Every node in every step renders -- no empty operator names anywhere in the tree.
        foreach (var step in steps)
            AssertWellFormed(step.Root);
    }

    [Fact]
    public void ShowPlan_ReturnsThePhysicalPlan()
    {
        var compilation = Compile("SELECT e.FirstName FROM Employees e");

        var plan = compilation.GetShowPlan()!;
        var lastStep = compilation.GetShowPlanSteps().Last();

        Assert.Equal("Physical", plan.Name);
        Assert.Equal(lastStep.Name, plan.Name);
    }

    [Fact]
    public void ShowPlan_ReturnsNoSteps_ForInvalidQuery()
    {
        var compilation = Compile("SELECT e.DoesNotExist FROM Employees e");

        Assert.Empty(compilation.GetShowPlanSteps());
        Assert.Null(compilation.GetShowPlan());
    }

    [Fact]
    public void ShowPlan_Physical_RendersHashMatch_ForEquiJoin()
    {
        var text = "SELECT o.OrderID FROM Orders o INNER JOIN [Order Details] od ON o.OrderID = od.OrderID";
        var physical = Compile(text).GetShowPlan()!;

        Assert.Contains(DescendantsAndSelf(physical.Root), n => n.OperatorName.StartsWith("Hash Match (Inner)"));
    }

    [Fact]
    public void ShowPlan_Logical_RendersApply_ForCorrelatedSubquery()
    {
        var text = "SELECT e.FirstName FROM Employees e WHERE EXISTS (SELECT * FROM Orders o WHERE o.EmployeeID = e.EmployeeID)";
        var unoptimized = Compile(text).GetShowPlanSteps().First();

        Assert.Equal("Unoptimized", unoptimized.Name);
        Assert.Contains(DescendantsAndSelf(unoptimized.Root), n => n.OperatorName.Contains("Apply"));
    }

    [Fact]
    public void ShowPlan_RendersBareExpression()
    {
        var steps = Compile("SELECT 1 + 2").GetShowPlanSteps().ToList();

        // A bare expression is wrapped in a one-row projection over a constant.
        Assert.Contains(DescendantsAndSelf(steps[0].Root), n => n.OperatorName == "Constant");
        Assert.Equal("Physical", steps[^1].Name);
    }

    private static void AssertWellFormed(ShowPlanNode node)
    {
        Assert.False(string.IsNullOrWhiteSpace(node.OperatorName));
        foreach (var child in node.Children)
            AssertWellFormed(child);
    }

    private static IEnumerable<ShowPlanNode> DescendantsAndSelf(ShowPlanNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var descendant in DescendantsAndSelf(child))
                yield return descendant;
        }
    }

    private static Compilation Compile(string text)
    {
        var syntaxTree = SyntaxTree.ParseQuery(text);
        return Compilation.Create(NorthwindDataContext.Instance, syntaxTree);
    }
}
