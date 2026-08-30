using System.Collections.Immutable;

using NQuery.CodeAnalysis;

namespace NQuery.Tests.CodeAnalysis.Binding;

public sealed class UnaryOperatorExpressionTests
{
    [Fact]
    public void UnaryOperator_DoesNotCauseCascadingErrors()
    {
        var syntaxTree = SyntaxTree.ParseExpression("+x");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();

        var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
    }
}
