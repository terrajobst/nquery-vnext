using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Tests.CodeAnalysis.Binding;

public class CaseExpressionTests
{
    [Fact]
    public void Case_DetectsConversionIssuesInWhenClause_WhenInSearchedCase()
    {
        var syntaxTree = SyntaxTree.ParseExpression("CASE '1' WHEN 1.0 THEN 1 WHEN 2 THEN 2 END");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

        Assert.Equal(2, diagnostics.Length);
        Assert.Equal(DiagnosticId.CannotApplyBinaryOperator, diagnostics[0].DiagnosticId);
        Assert.Equal(DiagnosticId.CannotApplyBinaryOperator, diagnostics[1].DiagnosticId);
    }

    [Fact]
    public void Case_DetectsConversionIssuesInThenClause_WhenInSearchedCase()
    {
        var syntaxTree = SyntaxTree.ParseExpression("CASE 1 WHEN 1 THEN '1' WHEN 2 THEN 2.0 ELSE 3 END");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

        Assert.Equal(2, diagnostics.Length);
        Assert.Equal(DiagnosticId.CannotConvert, diagnostics[0].DiagnosticId);
        Assert.Equal(DiagnosticId.CannotConvert, diagnostics[1].DiagnosticId);
    }

    [Fact]
    public void Case_AppliesConversionInWhenClause_WhenInSearchedCase()
    {
        var syntaxTree = SyntaxTree.ParseExpression("CASE 1 WHEN 1 THEN 1 WHEN 2.0 THEN 2 END");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

        var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root!);

        Assert.Empty(diagnostics);
        Assert.Equal(typeof(int), type);
    }

    [Fact]
    public void Case_AppliesConversionInThenClause_WhenInSearchedCase()
    {
        var syntaxTree = SyntaxTree.ParseExpression("CASE 1 WHEN 1 THEN 1 WHEN 2.0 THEN 2.0 END");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

        var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root!);

        Assert.Empty(diagnostics);
        Assert.Equal(typeof(double), type);
    }

    [Fact]
    public void Case_DetectsConversionIssuesInWhenClause_WhenInRegularCase()
    {
        var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1.0 = '1' THEN 1 WHEN TRUE = 2.0 THEN 2 END");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

        Assert.Equal(2, diagnostics.Length);
        Assert.Equal(DiagnosticId.CannotApplyBinaryOperator, diagnostics[0].DiagnosticId);
        Assert.Equal(DiagnosticId.CannotApplyBinaryOperator, diagnostics[1].DiagnosticId);
    }

    [Fact]
    public void Case_DetectsConversionIssuesInThenClause_WhenInRegularCase()
    {
        var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1 = 1 THEN '1' WHEN 2 = 2 THEN 2.0 ELSE 3 END");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

        Assert.Equal(2, diagnostics.Length);
        Assert.Equal(DiagnosticId.CannotConvert, diagnostics[0].DiagnosticId);
        Assert.Equal(DiagnosticId.CannotConvert, diagnostics[1].DiagnosticId);
    }

    [Fact]
    public void Case_AppliesConversionInWhenClause_WhenInRegularCase()
    {
        var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1 = 1.0 THEN 1 WHEN 2.0 = 2 THEN 2 END");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

        var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root!);

        Assert.Empty(diagnostics);
        Assert.Equal(typeof(int), type);
    }

    [Fact]
    public void Case_AppliesConversionInThenClause_WhenInRegularCase()
    {
        var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1 = 1 THEN 1 WHEN 2 = 2 THEN 2.0 END");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

        var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root!);

        Assert.Empty(diagnostics);
        Assert.Equal(typeof(double), type);
    }

    // A NULL branch with a typed sibling: the common type must be the sibling's type (NULL converts
    // to it), not the NULL type. Picking NULL as the common type used to emit a bogus "int -> NULL"
    // conversion that threw at codegen.
    [Fact]
    public void Case_WithNullThenAndTypedElse_HasElseType()
    {
        var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1 = 1 THEN NULL ELSE 2 END");
        var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

        var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root!);

        Assert.Empty(diagnostics);
        Assert.Equal(typeof(int), type);
    }

    // A taken NULL branch must evaluate to NULL, not the type's default. A non-nullable nested-scope
    // return type used to coalesce the NULL to 0.
    [Theory]
    [InlineData("CASE WHEN 1 = 1 THEN NULL ELSE 2 END", null)]
    [InlineData("CASE WHEN 1 = 0 THEN NULL ELSE 2 END", 2)]
    [InlineData("CASE WHEN 1 = 1 THEN 7 ELSE NULL END", 7)]
    public void Case_WithNullBranch_EvaluatesToNull(string text, object? expected)
    {
        var result = Expression<object>.Create(Catalog.Default, text).Evaluate();
        Assert.Equal(expected, result);
    }
}
