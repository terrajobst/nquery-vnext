using System.Collections.Immutable;

using NQuery.Syntax;

namespace NQuery.Tests.Binding
{
    public class InExpressionTests
    {
        [Fact]
        public void In_DetectsConversionIssues()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN (1, 2.0, '2')");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.CannotApplyBinaryOperator, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void In_AppliesConversion()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN (1, 3.0)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Empty(diagnostics);
        }

        [Fact]
        public void In_SupportsQuery()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN (SELECT Id FROM Table)");
            var compilation = Compilation.Empty.WithIdNameTable().WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();
            var expression = syntaxTree.Root.DescendantNodes().OfType<InQueryExpressionSyntax>().Single();

            Assert.Empty(diagnostics);
            Assert.Equal(typeof(bool), semanticModel.GetExpressionType(expression));
        }

        [Fact]
        public void In_SupportsQuery_WhenParenthesized()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN ((SELECT Id FROM Table))");
            var compilation = Compilation.Empty.WithIdNameTable().WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();
            var expression = syntaxTree.Root.DescendantNodes().OfType<InQueryExpressionSyntax>().Single();

            Assert.Empty(diagnostics);
            Assert.Equal(typeof(bool), semanticModel.GetExpressionType(expression));
        }
    }
}