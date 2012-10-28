using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests
{
    [TestClass]
    public class TopTests
    {
        [TestMethod]
        public void Top_WithMissingWithKeyword_IsParsedCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT TOP 1 TIES NULL");
            var query = (SelectQuerySyntax) syntaxTree.Root.Root;
            var diagnostics = syntaxTree.GetDiagnostics().ToArray();

            Assert.IsFalse(query.SelectClause.TopClause.TiesKeyword.IsMissing);
            Assert.IsTrue(query.SelectClause.TopClause.WithKeyword.IsMissing);
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Top_WithMissingTiesKeyword_IsParsedCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT TOP 1 WITH NULL");
            var query = (SelectQuerySyntax)syntaxTree.Root.Root;
            var diagnostics = syntaxTree.GetDiagnostics().ToArray();

            Assert.IsTrue(query.SelectClause.TopClause.TiesKeyword.IsMissing);
            Assert.IsFalse(query.SelectClause.TopClause.WithKeyword.IsMissing);
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Top_WithInvalidInt_IsParsedAndBoundCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT TOP 1.5 NULL");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var smanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(smanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidInteger, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Top_WithInvalidLiteral_IsParsedAndBoundCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT TOP 'text'");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var smanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(smanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }
    }
}