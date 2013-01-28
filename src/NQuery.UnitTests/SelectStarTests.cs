using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests
{
    [TestClass]
    public class SelectStarTests
    {
        [TestMethod]
        public void SelectStar_Disallowed_WhenNoTablesSpecified()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT *");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.MustSpecifyTableToSelectFrom, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void SelectStar_Disallowed_WhenNoTablesSpecified_UnlessInExists()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 'Test' WHERE EXISTS (SELECT *)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }
    }
}