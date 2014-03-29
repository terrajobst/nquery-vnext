using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests.Binding
{
    [TestClass]
    public class BetweenExpressionTests
    {
        [TestMethod]
        public void Between_DetectsConversionIssues()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 BETWEEN '1' AND 2.0");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.CannotApplyBinaryOperator, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Between_AppliesConversion()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 BETWEEN 1 AND 2.0");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }
    }
}