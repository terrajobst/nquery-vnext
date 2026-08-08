using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Tests.CodeAnalysis.Binding;

public class NullIfExpressionTests
{
    [Fact]
    public void NullIf_DetectsConversionIssues()
    {
        var syntaxTree = SyntaxTree.ParseExpression("NULLIF(1, '2')");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.CannotConvert, diagnostics[0].DiagnosticId);
    }

    [Fact]
    public void NullIf_AppliesConversion()
    {
        var syntaxTree = SyntaxTree.ParseExpression("NULLIF(1, 3.0)");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

        var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root!);

        Assert.Empty(diagnostics);
        Assert.Equal(typeof(double), type);
    }

    // SQL-standard NULLIF: NULL when the operands are equal, otherwise the left value -- including
    // when the comparison is unknown because the right operand is NULL. The previous
    // "WHEN left != right THEN left" lowering wrongly returned NULL for NULLIF(2, NULL).
    [Theory]
    [InlineData("NULLIF(2, 2)", null)]
    [InlineData("NULLIF(2, 3)", 2)]
    [InlineData("NULLIF(2, NULL)", 2)]
    [InlineData("NULLIF(NULL, 2)", null)]
    [InlineData("NULLIF(NULL, NULL)", null)]
    public void NullIf_EvaluatesToSqlStandard(string text, object? expected)
    {
        var result = Expression<object>.Create(Catalog.Default, text).Evaluate();
        Assert.Equal(expected, result);
    }
}
