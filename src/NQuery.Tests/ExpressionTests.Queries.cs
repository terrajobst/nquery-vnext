using NQuery.CodeAnalysis.Symbols;
using NQuery.Metadata;

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
}
