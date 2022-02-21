using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.Tests.Binding
{
    public sealed class CastExpressionTests
    {
        [Fact]
        public void Cast_DoesNotCauseCascadingErrors_WhenExpressionIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CAST(foo AS STRING)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var returnType = semanticMoel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(typeof(string), returnType);
            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Cast_DoesNotCauseCascadingErrors_WhenTypeIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CAST(1.0 AS foo)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.UndeclaredType, diagnostics[0].DiagnosticId);
        }
    }
}