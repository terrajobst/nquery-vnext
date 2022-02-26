using System.Collections.Immutable;

using NQuery.Syntax;

namespace NQuery.Tests.Binding
{
    public class CaseExpressionTests
    {
        [Fact]
        public void Case_DetectsConversionIssuesInWhenClause_WhenInSearchedCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE '1' WHEN 1.0 THEN 1 WHEN 2 THEN 2 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

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
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

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
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax) syntaxTree.Root.Root);

            Assert.Empty(diagnostics);
            Assert.Equal(typeof(int), type);
        }

        [Fact]
        public void Case_AppliesConversionInThenClause_WhenInSearchedCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE 1 WHEN 1 THEN 1 WHEN 2.0 THEN 2.0 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);

            Assert.Empty(diagnostics);
            Assert.Equal(typeof(double), type);
        }

        [Fact]
        public void Case_DetectsConversionIssuesInWhenClause_WhenInRegularCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1.0 = '1' THEN 1 WHEN TRUE = 2.0 THEN 2 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

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
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

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
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);

            Assert.Empty(diagnostics);
            Assert.Equal(typeof(int), type);
        }

        [Fact]
        public void Case_AppliesConversionInThenClause_WhenInRegularCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1 = 1 THEN 1 WHEN 2 = 2 THEN 2.0 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);

            Assert.Empty(diagnostics);
            Assert.Equal(typeof(double), type);
        }
    }
}