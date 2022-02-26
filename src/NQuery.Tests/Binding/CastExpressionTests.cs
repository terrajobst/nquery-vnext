using System.Collections.Immutable;

using NQuery.Syntax;

namespace NQuery.Tests.Binding
{
    public sealed class CastExpressionTests
    {
        [Fact]
        public void Cast_DoesNotCauseCascadingErrors_WhenExpressionIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CAST(foo AS STRING)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var returnType = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(typeof(string), returnType);
            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Cast_DoesNotCauseCascadingErrors_WhenTypeIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CAST(1.0 AS foo)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.UndeclaredType, diagnostics[0].DiagnosticId);
        }
    }
}