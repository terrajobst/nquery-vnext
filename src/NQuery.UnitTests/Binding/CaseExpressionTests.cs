using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Syntax;

namespace NQuery.UnitTests.Binding
{
    [TestClass]
    public class CaseExpressionTests
    {
        [TestMethod]
        public void Case_DetectsConversionIssuesInWhenClause_WhenInSearchedCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE '1' WHEN 1.0 THEN 1 WHEN 2 THEN 2 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(2, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.CannotApplyBinaryOperator, diagnostics[0].DiagnosticId);
            Assert.AreEqual(DiagnosticId.CannotApplyBinaryOperator, diagnostics[1].DiagnosticId);
        }

        [TestMethod]
        public void Case_DetectsConversionIssuesInThenClause_WhenInSearchedCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE 1 WHEN 1 THEN '1' WHEN 2 THEN 2.0 ELSE 3 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(2, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.CannotConvert, diagnostics[0].DiagnosticId);
            Assert.AreEqual(DiagnosticId.CannotConvert, diagnostics[1].DiagnosticId);
        }

        [TestMethod]
        public void Case_AppliesConversionInWhenClause_WhenInSearchedCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE 1 WHEN 1 THEN 1 WHEN 2.0 THEN 2 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax) syntaxTree.Root.Root);

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(typeof(int), type);
        }

        [TestMethod]
        public void Case_AppliesConversionInThenClause_WhenInSearchedCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE 1 WHEN 1 THEN 1 WHEN 2.0 THEN 2.0 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(typeof(double), type);
        }

        [TestMethod]
        public void Case_DetectsConversionIssuesInWhenClause_WhenInRegularCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1.0 = '1' THEN 1 WHEN TRUE = 2.0 THEN 2 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(2, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.CannotApplyBinaryOperator, diagnostics[0].DiagnosticId);
            Assert.AreEqual(DiagnosticId.CannotApplyBinaryOperator, diagnostics[1].DiagnosticId);
        }

        [TestMethod]
        public void Case_DetectsConversionIssuesInThenClause_WhenInRegularCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1 = 1 THEN '1' WHEN 2 = 2 THEN 2.0 ELSE 3 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(2, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.CannotConvert, diagnostics[0].DiagnosticId);
            Assert.AreEqual(DiagnosticId.CannotConvert, diagnostics[1].DiagnosticId);
        }

        [TestMethod]
        public void Case_AppliesConversionInWhenClause_WhenInRegularCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1 = 1.0 THEN 1 WHEN 2.0 = 2 THEN 2 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(typeof(int), type);
        }

        [TestMethod]
        public void Case_AppliesConversionInThenClause_WhenInRegularCase()
        {
            var syntaxTree = SyntaxTree.ParseExpression("CASE WHEN 1 = 1 THEN 1 WHEN 2 = 2 THEN 2.0 END");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            var type = semanticModel.GetExpressionType((ExpressionSyntax)syntaxTree.Root.Root);

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(typeof(double), type);
        }
    }
}