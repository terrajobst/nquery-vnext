using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Syntax;

namespace NQuery.UnitTests.Binding
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
        public void In_SupportsQuery()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN (SELECT Id FROM Table)");
            var compilation = Compilation.Empty.WithIdNameTable().WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();
            var expression = syntaxTree.Root.DescendantNodes().OfType<InQueryExpressionSyntax>().Single();

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(typeof(bool), semanticModel.GetExpressionType(expression));
        }

        [TestMethod]
        public void In_SupportsQuery_WhenParenthesized()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 IN ((SELECT Id FROM Table))");
            var compilation = Compilation.Empty.WithIdNameTable().WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();
            var expression = syntaxTree.Root.DescendantNodes().OfType<InQueryExpressionSyntax>().Single();

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(typeof(bool), semanticModel.GetExpressionType(expression));
        }
    }
}