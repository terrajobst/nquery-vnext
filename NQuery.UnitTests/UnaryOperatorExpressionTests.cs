using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace NQuery.Language.UnitTests
{
    [TestClass]
    public sealed class UnaryOperatorExpressionTests
    {
        [TestMethod]
        public void UnaryOperator_DoesNotCauseCascadingErrors()
        {
            var syntaxTree = SyntaxTree.ParseExpression("+x");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();
            
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}