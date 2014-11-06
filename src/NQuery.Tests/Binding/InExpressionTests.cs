using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.UnitTests.Binding
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

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.CannotApplyBinaryOperator, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void In_AppliesConversion()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN (1, 3.0)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(0, diagnostics.Length);
        }

        [Fact]
        public void In_SupportsQuery()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN (SELECT Id FROM Table)");
            var compilation = Compilation.Empty.WithIdNameTable().WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();
            var expression = syntaxTree.Root.DescendantNodes().OfType<InQueryExpressionSyntax>().Single();

            Assert.Equal(0, diagnostics.Length);
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

            Assert.Equal(0, diagnostics.Length);
            Assert.Equal(typeof(bool), semanticModel.GetExpressionType(expression));
        }
    }
}