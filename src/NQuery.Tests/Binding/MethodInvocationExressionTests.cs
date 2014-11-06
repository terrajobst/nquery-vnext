using System;
using System.Collections.Immutable;
using System.Linq;

using Xunit;

namespace NQuery.UnitTests.Binding
{
    public sealed class MethodInvocationExressionTests
    {
        [Fact]
        public void FunctionInvocation_DoesNotCauseCascadingErrors_WhenTargetIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x.Substring(1.0, 2)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void FunctionInvocation_DoesNotCauseCascadingErrors_WhenAnArgumentIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("'x'.Substring(1.0, bar)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}