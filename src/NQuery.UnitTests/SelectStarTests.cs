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

        [TestMethod]
        public void SelectStar_HasAssociatedTable()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.* FROM Table t");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var selectStar = compilation.SyntaxTree.Root
                                        .DescendantNodes()
                                        .OfType<WildcardSelectColumnSyntax>()
                                        .Single();
            var tableInstance = compilation.SyntaxTree.Root
                                           .DescendantNodes()
                                           .OfType<NamedTableReferenceSyntax>()
                                           .Single();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            var tableReferenceSymbol = semanticModel.GetDeclaredSymbol(tableInstance);
            var selectStarSymbol = semanticModel.GetTableInstance(selectStar);

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(tableReferenceSymbol, selectStarSymbol);
        }

        [TestMethod]
        public void SelectStar_HasAssociatedTable_UnlessNoTableIsSpecified()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT * FROM Table t");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var selectStar = compilation.SyntaxTree.Root
                                        .DescendantNodes()
                                        .OfType<WildcardSelectColumnSyntax>()
                                        .Single();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            var selectStarSymbol = semanticModel.GetTableInstance(selectStar);

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(null, selectStarSymbol);
        }

        [TestMethod]
        public void SelectStar_HasAssociatedTableColumns()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT * FROM Table");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var selectStar = compilation.SyntaxTree.Root
                                        .DescendantNodes()
                                        .OfType<WildcardSelectColumnSyntax>()
                                        .Single();
            var tableInstance = compilation.SyntaxTree.Root
                                           .DescendantNodes()
                                           .OfType<NamedTableReferenceSyntax>()
                                           .Single();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            var tableSymbols = semanticModel.GetDeclaredSymbol(tableInstance).ColumnInstances;
            var selectStartSymbols = semanticModel.GetColumnInstances(selectStar).ToArray();

            Assert.AreEqual(0, diagnostics.Length);
            CollectionAssert.AreEqual(tableSymbols, selectStartSymbols);
        }

        [TestMethod]
        public void SelectStar_HasAssociatedQueryTableColumns()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT * FROM Table");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var selectStar = compilation.SyntaxTree.Root
                                        .DescendantNodes()
                                        .OfType<WildcardSelectColumnSyntax>()
                                        .Single();
            var tableInstance = compilation.SyntaxTree.Root
                                           .DescendantNodes()
                                           .OfType<NamedTableReferenceSyntax>()
                                           .Single();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            var tableColumnNames = semanticModel.GetDeclaredSymbol(tableInstance)
                                                .ColumnInstances
                                                .Select(c => c.Name)
                                                .ToArray();
            var selectStarColumnNames = semanticModel.GetDeclaredSymbols(selectStar)
                                                     .Select(qc => qc.Name)
                                                     .ToArray();

            Assert.AreEqual(0, diagnostics.Length);
            CollectionAssert.AreEqual(tableColumnNames, selectStarColumnNames);
        }
    }
}