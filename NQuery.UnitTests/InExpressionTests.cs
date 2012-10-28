using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Language.UnitTests
{
    [TestClass]
    public class InExpressionTests
    {
        [TestMethod]
        public void In_DetectsConversionIssues()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN (1, 2.0, '2')");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.CannotApplyBinaryOperator, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void In_AppliesConversion()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN (1, 3.0)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }
        [TestMethod]
        public void In__DetectsTooFewArguments_WhenNoArgumentIsProvided()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN ()");
            var diagnostics = syntaxTree.GetDiagnostics().ToArray();
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }
    }

}