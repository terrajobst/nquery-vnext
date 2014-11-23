using System;
using System.Collections.Immutable;
using System.Linq;

using Xunit;

namespace NQuery.Tests.Binding
{
    public sealed class PropertyAccessExressionTests
    {
        [Fact]
        public void PropertyAccess_DetectsMissingParentheses_WhenReferringToMethod()
        {
            var syntaxTree = SyntaxTree.ParseExpression("'test'.SUBSTRING");
            var compilation = Compilation.Empty
                .WithDataContext(DataContext.Default)
                .WithSyntaxTree(syntaxTree);

            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.InvocationRequiresParenthesis, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void PropertyAccess_DoesNotCauseCascadingErrors_WhenTargetIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("foo.Bar");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}