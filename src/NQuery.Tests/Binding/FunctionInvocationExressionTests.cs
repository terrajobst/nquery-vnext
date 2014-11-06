using System;
using System.Collections.Immutable;
using System.Linq;

using Xunit;

namespace NQuery.UnitTests.Binding
{
    public sealed class FunctionInvocationExressionTests
    {
        [Fact]
        public void FunctionInvocation_DoesNotCauseCascadingErrors_WhenAnArgmentIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("foo(1.0, bar)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}