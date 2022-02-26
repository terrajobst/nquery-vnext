using System.Collections.Immutable;

using Xunit;

namespace NQuery.Tests.Binding
{
    public sealed class UnaryOperatorExpressionTests
    {
        [Fact]
        public void UnaryOperator_DoesNotCauseCascadingErrors()
        {
            var syntaxTree = SyntaxTree.ParseExpression("+x");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}