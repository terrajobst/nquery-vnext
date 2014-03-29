using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Syntax;

namespace NQuery.UnitTests.Binding
{
    [TestClass]
    public sealed class CastExpressionTests
    {
        [TestMethod]
        public void Cast_DoesNotCauseCascadingErrors_WhenExpressionIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CAST(foo AS STRING)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var returnType = semanticMoel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();

            Assert.AreEqual(typeof(string), returnType);
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Cast_DoesNotCauseCascadingErrors_WhenTypeIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CAST(1.0 AS foo)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.UndeclaredType, diagnostics[0].DiagnosticId);
        }
    }
}