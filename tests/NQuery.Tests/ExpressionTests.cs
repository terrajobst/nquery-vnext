using NQuery.CodeAnalysis;
using NQuery.Metadata;

namespace NQuery.Tests;

public partial class ExpressionTests
{
    [Fact]
    public void Expression_AllowsConstructingInvalidExpressions()
    {
        var catalog = Catalog.Empty;
        var fooBar = "FOO BAR";

        var query = Expression<object>.Create(catalog, fooBar);

        Assert.Equal(catalog, query.Catalog);
        Assert.Equal(fooBar, query.Text);
    }

    [Fact]
    public void Expression_CtorThrows_IfCatalogIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            Expression<object>.Create(null!, string.Empty);
        });
    }

    [Fact]
    public void Expression_CtorThrows_IfTextIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            Expression<object>.Create(Catalog.Empty, null!);
        });
    }

    [Fact]
    public void Expression_CtorThrows_IfTIsNotAssignableFromTargetType()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Expression<bool>.Create(Catalog.Empty, "", false, typeof(int));
        });
    }

    [Fact]
    public void Expression_ResolveThrows_IfExpressionCannotBeParsed()
    {
        var expression = Expression<bool>.Create(Catalog.Empty, "foo;");

        try
        {
            expression.Resolve();
        }
        catch (CompilationException ex)
        {
            Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.IllegalInputCharacter));
            Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared));
        }
    }

    [Fact]
    public void Expression_EvaluateThrows_IfExpressionCannotBeParsed()
    {
        var expression = Expression<bool>.Create(Catalog.Empty, "bar;");

        try
        {
            expression.Evaluate();
        }
        catch (CompilationException ex)
        {
            Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.IllegalInputCharacter));
            Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared));
        }
    }

    [Fact]
    public void Expression_ResolveReturnsValue()
    {
        var expression = Expression<double>.Create(Catalog.Empty, "1 + 1.0");
        var result = expression.Resolve();
        Assert.Equal(typeof(double), result);
    }

    [Fact]
    public void Expression_EvaluateReturnsValue()
    {
        var expression = Expression<double>.Create(Catalog.Empty, "1 + 1.0");
        var result = expression.Evaluate();
        Assert.Equal(2.0, result);
    }

    [Fact]
    public void Expression_Evaluate_HonorsNullValue()
    {
        const int nullValue = 42;
        var expression = Expression<int>.Create(Catalog.Empty, "NULL", nullValue);
        var result = expression.Evaluate();
        Assert.Equal(nullValue, result);
    }

    [Fact]
    public void Expression_Resolve_ResolvesVariableType()
    {
        var sum = VariableDefinition.Create("Sum", typeof(int));
        var count = VariableDefinition.Create("Count", typeof(int));
        var catalog = Catalog.Empty.AddVariables(sum, count);
        var expression = Expression<object>.Create(catalog, "@Sum / @Count");

        Assert.Equal(typeof(int), expression.Resolve());
    }

    [Fact]
    public void Expression_Evaluate_ReadsLiveVariableValues()
    {
        // The aggregate definitions (e.g. AVG) reuse one compiled expression across rows,
        // mutating the variables between Evaluate calls -- so each evaluation must read the
        // variable's current value rather than the value captured at compile time.
        var sum = VariableDefinition.Create("Sum", typeof(int));
        var count = VariableDefinition.Create("Count", typeof(int));
        var catalog = Catalog.Empty.AddVariables(sum, count);
        var expression = Expression<int>.Create(catalog, "@Sum / @Count");

        sum.Value = 10;
        count.Value = 2;
        Assert.Equal(5, expression.Evaluate());

        sum.Value = 30;
        count.Value = 3;
        Assert.Equal(10, expression.Evaluate());
    }
}
