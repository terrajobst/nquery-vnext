using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests
{
    [TestClass]
    public sealed class MethodInvocationExressionTests
    {
        [TestMethod]
        public void FunctionInvocation_DoesNotCauseCascadingErrors_WhenTargetIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x.Substring(1.0, 2)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void FunctionInvocation_DoesNotCauseCascadingErrors_WhenAnArgumentIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("'x'.Substring(1.0, bar)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}