using System.Collections.Immutable;

using NQuery.Syntax;

using Xunit;

namespace NQuery.Tests.Binding
{
    public class CoalesceExpressionTests
    {
        [Fact]
        public void Coalesce_DetectsConversionIssues()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COALESCE(1, '2', 3.0)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.CannotConvert, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Coalesce_AppliesConversion()
        {
            var syntaxTree = SyntaxTree.ParseExpression("COALESCE(1, 3.0)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax) syntaxTree.Root.Root);

            Assert.Empty(diagnostics);
            Assert.Equal(typeof(double), type);
        }
    }
}