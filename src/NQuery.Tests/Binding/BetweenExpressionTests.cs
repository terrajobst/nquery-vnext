using System;
using System.Collections.Immutable;
using System.Linq;

using Xunit;

namespace NQuery.Tests.Binding
{
    public class BetweenExpressionTests
    {
        [Fact]
        public void Between_DetectsConversionIssues()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 BETWEEN '1' AND 2.0");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.CannotApplyBinaryOperator, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Between_AppliesConversion()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 BETWEEN 1 AND 2.0");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Empty(diagnostics);
        }
    }
}