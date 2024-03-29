using System.Collections.Immutable;

namespace NQuery.Tests.Binding
{
    public sealed class MethodInvocationExpressionTests
    {
        [Fact]
        public void FunctionInvocation_DoesNotCauseCascadingErrors_WhenTargetIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x.Substring(1.0, 2)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void FunctionInvocation_DoesNotCauseCascadingErrors_WhenAnArgumentIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("'x'.Substring(1.0, bar)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}