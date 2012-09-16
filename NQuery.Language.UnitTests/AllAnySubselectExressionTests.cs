using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Language.UnitTests
{
    [TestClass]
    public sealed class AllAnySubselectExressionTests
    {
        [TestMethod]
        public void AllAnySubselect_DoesNotCauseCascadingErrors_WhenAnLeftIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("foo >= ALL (SELECT 'value')");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);            
        }

        [TestMethod]
        public void AllAnySubselect_DoesNotCauseCascadingErrors_WhenAnRightIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("'value' >= ALL (SELECT foo)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);            
        }
    }
}