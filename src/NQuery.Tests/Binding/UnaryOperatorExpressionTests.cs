using System;
using System.Collections.Immutable;
using System.Linq;

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
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}