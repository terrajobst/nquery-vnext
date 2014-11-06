using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.UnitTests.Binding
{
    public class NullIfExpressionTests
    {
        [Fact]
        public void NullIf_DetectsConversionIssues()
        {
            var syntaxTree = SyntaxTree.ParseExpression("NULLIF(1, '2')");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.CannotConvert, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void NullIf_AppliesConversion()
        {
            var syntaxTree = SyntaxTree.ParseExpression("NULLIF(1, 3.0)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);

            Assert.Equal(0, diagnostics.Length);
            Assert.Equal(typeof(double), type);
        }
    }
}