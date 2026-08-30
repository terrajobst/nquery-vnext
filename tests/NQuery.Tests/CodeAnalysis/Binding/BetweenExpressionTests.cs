using System.Collections.Immutable;

using NQuery.CodeAnalysis;

namespace NQuery.Tests.CodeAnalysis.Binding;

public class BetweenExpressionTests
{
    [Fact]
    public void Between_DetectsConversionIssues()
    {
        var syntaxTree = SyntaxTree.ParseExpression("1 BETWEEN '1' AND 2.0");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.CannotApplyBinaryOperator, diagnostics[0].DiagnosticId);
    }

    [Fact]
    public void Between_AppliesConversion()
    {
        var syntaxTree = SyntaxTree.ParseExpression("1 BETWEEN 1 AND 2.0");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics();

        Assert.Empty(diagnostics);
    }
}
