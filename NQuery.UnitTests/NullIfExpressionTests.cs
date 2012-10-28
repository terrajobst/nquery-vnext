using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Language.UnitTests
{
    [TestClass]
    public class NullIfExpressionTests
    {
        [TestMethod]
        public void NullIf_DetectsConversionIssues()
        {
            var syntaxTree = SyntaxTree.ParseExpression("NULLIF(1, '2')");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.CannotConvert, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void NullIf_AppliesConversion()
        {
            var syntaxTree = SyntaxTree.ParseExpression("NULLIF(1, 3.0)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(typeof(double), type);
        }
    }
}