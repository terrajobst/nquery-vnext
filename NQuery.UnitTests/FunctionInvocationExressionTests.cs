using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests
{
    [TestClass]
    public sealed class FunctionInvocationExressionTests
    {
        [TestMethod]
        public void FunctionInvocation_DoesNotCauseCascadingErrors_WhenAnArgmentIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("foo(1.0, bar)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}