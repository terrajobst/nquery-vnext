using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace NQuery.Language.UnitTests
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

            Assert.IsFalse(query.SelectClause.TopClause.TiesKeyword.Value.IsMissing);
            Assert.IsTrue(query.SelectClause.TopClause.WithKeyword.Value.IsMissing);
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Top_WithMissingTiesKeyword_IsParsedCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT TOP 1 WITH NULL");
            var query = (SelectQuerySyntax)syntaxTree.Root.Root;
            var diagnostics = syntaxTree.GetDiagnostics().ToArray();

            Assert.IsTrue(query.SelectClause.TopClause.TiesKeyword.Value.IsMissing);
            Assert.IsFalse(query.SelectClause.TopClause.WithKeyword.Value.IsMissing);
            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }
    }
}