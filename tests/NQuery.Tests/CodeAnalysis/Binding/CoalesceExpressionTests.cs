using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Tests.CodeAnalysis.Binding;

public class CoalesceExpressionTests
{
    [Fact]
    public void Coalesce_DetectsConversionIssues()
    {
        var syntaxTree = SyntaxTree.ParseExpression("COALESCE(1, '2', 3.0)");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.CannotConvert, diagnostics[0].DiagnosticId);
    }

    [Fact]
    public void Coalesce_AppliesConversion()
    {
        var syntaxTree = SyntaxTree.ParseExpression("COALESCE(1, 3.0)");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics();

        var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root!);

        Assert.Empty(diagnostics);
        Assert.Equal(typeof(double), type);
    }
}
