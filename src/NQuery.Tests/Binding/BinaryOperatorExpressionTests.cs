using System.Collections.Immutable;

using Xunit;

namespace NQuery.Tests.Binding
{
    public sealed class BinaryOperatorExpressionTests
    {
        [Fact]
        public void BinaryOperator_DoesNotCauseCascadingErrors_WhenBothOperandsAreUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x + y");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(2, diagnostics.Length);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[1].DiagnosticId);
        }

        [Fact]
        public void BinaryOperator_DoesNotCauseCascadingErrors_WhenLeftOperandsIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x + 1");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void BinaryOperator_DoesNotCauseCascadingErrors_WhenRightOperandsIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 + y");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}