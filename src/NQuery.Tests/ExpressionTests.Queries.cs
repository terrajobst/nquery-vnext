using NQuery.CodeAnalysis.Symbols;
using NQuery.Metadata;
using NQuery.Northwind;

namespace NQuery.Tests;

public partial class ExpressionTests
{
    [Fact]
    public void Expression_Queries_SingleRowSubselect()
    {
        var catalog = NorthwindCatalog.Instance;
        var text = "(SELECT LastName FROM Employees WHERE FirstName = 'Margaret')";
        var expression = Expression<string>.Create(catalog, text);
        var result = expression.Evaluate();

        Assert.Equal("Peacock", result);
    }

    [Fact]
    public void Expression_Queries_Exists()
    {
        var catalog = NorthwindCatalog.Instance;
        var text = "EXISTS (SELECT * FROM Employees WHERE FirstName = 'Margaret')";
        var expression = Expression<bool>.Create(catalog, text);
        var result = expression.Evaluate();

        Assert.True(result);
    }

    [Fact]
    public void Expression_Queries_Exists_NoFilter()
    {
        var catalog = NorthwindCatalog.Instance;
        var text = "EXISTS (SELECT * FROM Employees)";
        var expression = Expression<bool>.Create(catalog, text);
        var result = expression.Evaluate();

        Assert.True(result);
    }

    [Fact]
    public void Expression_Queries_All()
    {
        var catalog = NorthwindCatalog.Instance;
        var text = "10 >= ALL (SELECT EmployeeId FROM Employees)";
        var expression = Expression<bool>.Create(catalog, text);
        var result = expression.Evaluate();

        Assert.True(result);
    }

    [Fact]
    public void Expression_Queries_Any()
    {
        var name = VariableDefinition.Create("name", typeof(string), "Margaret");
        var catalog = NorthwindCatalog.Instance.AddVariables(name);
        var text = "'London' = ANY (SELECT City FROM Employees)";
        var expression = Expression<bool>.Create(catalog, text);
        var result = expression.Evaluate();

        Assert.True(result);
    }

    // A scalar subquery used as an operand that the binder lowers by *duplicating* that operand
    // -- COALESCE, NULLIF, and the input of a simple (searched) CASE -- must still compile and
    // run. Each lowering references the same BoundSingleRowSubselect node more than once (e.g.
    // COALESCE(x, y) => CASE WHEN x IS NOT NULL THEN x ELSE y END references x twice), and the
    // algebrizer visits every occurrence. Because a subquery's value slots are minted per symbol,
    // each visit emitted a TableScan minting the *same* slots, so the algebrized plan defined a
    // slot from two operators and tripped plan verification with "value slot ... is introduced by
    // more than one operator". These run against the typed Northwind catalog.
    [Fact]
    public void Expression_Queries_SingleRowSubselect_InCoalesce()
    {
        var text = "COALESCE((SELECT MAX(EmployeeID) FROM Employees), 10)";
        var expression = Expression<int>.Create(NorthwindCatalog.InstanceTyped, text);

        Assert.Equal(9, expression.Evaluate());
    }

    [Fact]
    public void Expression_Queries_SingleRowSubselect_InNullIf()
    {
        var text = "NULLIF((SELECT MAX(EmployeeID) FROM Employees), 0)";
        var expression = Expression<int>.Create(NorthwindCatalog.InstanceTyped, text);

        Assert.Equal(9, expression.Evaluate());
    }

    [Fact]
    public void Expression_Queries_SingleRowSubselect_AsSimpleCaseInput()
    {
        // Two WHEN labels, so the lowered form references the subquery input twice.
        var text = "CASE (SELECT MAX(EmployeeID) FROM Employees) WHEN 1 THEN 'one' WHEN 9 THEN 'nine' ELSE 'other' END";
        var expression = Expression<string>.Create(NorthwindCatalog.InstanceTyped, text);

        Assert.Equal("nine", expression.Evaluate());
    }
}
