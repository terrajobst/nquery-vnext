using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Tests.CodeAnalysis.Binding;

public sealed class AllAnySubselectExpressionTests
{
    [Fact]
    public void AllAnySubselect_DoesNotCauseCascadingErrors_WhenAnLeftIsUnresolved()
    {
        var syntaxTree = SyntaxTree.ParseExpression("foo >= ALL (SELECT 'value')");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();

        var diagnostics = semanticModel.GetDiagnostics();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
    }

    [Fact]
    public void AllAnySubselect_DoesNotCauseCascadingErrors_WhenAnRightIsUnresolved()
    {
        var syntaxTree = SyntaxTree.ParseExpression("'value' >= ALL (SELECT foo)");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();

        var diagnostics = semanticModel.GetDiagnostics();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
    }

    [Fact]
    public void AllAnySubselect_DetectsInvalidOperator()
    {
        var syntaxTree = SyntaxTree.ParseExpression("'value' + ALL (SELECT foo)");
        var diagnostics = syntaxTree.GetDiagnostics().ToImmutableArray();

        Assert.Single(diagnostics);
        Assert.Equal(DiagnosticId.InvalidOperatorForAllAny, diagnostics[0].DiagnosticId);
    }

    [Fact]
    public void AllAnySubselect_ConvertsLeftSide()
    {
        var compilation = CompilationFactory.CreateExpression("1 >= ALL (SELECT EmployeeId * 2.0 FROM Employees)");
        var expression = (ExpressionSyntax)compilation.SyntaxTree.Root.Root!;
        var semanticModel = compilation.GetSemanticModel();
        var resultType = semanticModel.GetExpressionType(expression);
        var diagnostics = semanticModel.GetDiagnostics();

        Assert.Equal(typeof(bool), resultType);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void AllAnySubselect_ConvertsRightSide()
    {
        var compilation = CompilationFactory.CreateExpression("1.0 >= ALL (SELECT EmployeeId FROM Employees)");
        var expression = (ExpressionSyntax)compilation.SyntaxTree.Root.Root!;
        var semanticModel = compilation.GetSemanticModel();
        var resultType = semanticModel.GetExpressionType(expression);
        var diagnostics = semanticModel.GetDiagnostics();

        Assert.Equal(typeof(bool), resultType);
        Assert.Empty(diagnostics);
    }
}
