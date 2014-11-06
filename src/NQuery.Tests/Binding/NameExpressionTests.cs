using System;
using System.Collections.Immutable;
using System.Linq;

using Xunit;

namespace NQuery.UnitTests.Binding
{
    public class NameExpressionTests
    {
        [Fact]
        public void Name_DetectsMissingParentheses_WhenReferringToFunction()
        {
            var syntaxTree = SyntaxTree.ParseExpression("SUBSTRING");
            var compilation = Compilation.Empty
                .WithDataContext(DataContext.Default)
                .WithSyntaxTree(syntaxTree);

            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.InvocationRequiresParenthesis, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Name_DetectsMissingParentheses_WhenReferringToAggregate()
        {
            var syntaxTree = SyntaxTree.ParseExpression("MAX");
            var compilation = Compilation.Empty
                .WithDataContext(DataContext.Default)
                .WithSyntaxTree(syntaxTree);

            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.InvocationRequiresParenthesis, diagnostics[0].DiagnosticId);
        }
    }
}