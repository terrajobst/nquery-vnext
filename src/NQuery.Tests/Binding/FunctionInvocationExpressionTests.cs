using System.Collections.Immutable;

using Xunit;

namespace NQuery.Tests.Binding
{
    public sealed class FunctionInvocationExpressionTests
    {
        [Fact]
        public void FunctionInvocation_DoesNotCauseCascadingErrors_WhenAnArgumentIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("foo(1.0, bar)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}