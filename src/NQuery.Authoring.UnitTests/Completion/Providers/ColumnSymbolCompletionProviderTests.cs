using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Completion;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.UnitTests.Completion.Providers
{
    [TestClass]
    public class ColumnSymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, string tableInstanceName, string columnName)
        {
            TableColumnInstanceSymbol column;
            CompletionItem columnItem;
            SymbolMarkup columnMarkup;
            GetCompletionData(query, tableInstanceName, columnName, out column, out columnItem, out columnMarkup);

            Assert.AreEqual(NQueryGlyph.Column, columnItem.Glyph);
            Assert.AreEqual(column.Name, columnItem.DisplayText);
            Assert.AreEqual(columnMarkup.ToString(), columnItem.Description);
            Assert.AreEqual(column, columnItem.Symbol);
        }

        private static void AssertIsAmbiguousMatch(string query, string tableInstanceName, string columnName)
        {
            TableColumnInstanceSymbol column;
            CompletionItem columnItem;
            SymbolMarkup columnMarkup;
            GetCompletionData(query, tableInstanceName, columnName, out column, out columnItem, out columnMarkup);

            Assert.AreEqual(NQueryGlyph.AmbiguousName, columnItem.Glyph);
            Assert.AreEqual(column.Name, columnItem.DisplayText);
            Assert.IsTrue(columnItem.Description.StartsWith("Ambiguous Name:"));
            Assert.AreEqual(null, columnItem.Symbol);
        }

        private static void GetCompletionData(string query, string tableInstanceName, string columnName, out TableColumnInstanceSymbol column, out CompletionItem columnItem, out SymbolMarkup columnMarkup)
        {
            var completionModel = GetCompletionModel(query);
            var semanticModel = completionModel.SemanticModel;
            var syntaxTree = semanticModel.Compilation.SyntaxTree;

            var tableReference = syntaxTree.Root.DescendantNodesAndSelf()
                .OfType<NamedTableReferenceSyntax>()
                .Select(semanticModel.GetDeclaredSymbol)
                .Single(s => s != null && s.Name == tableInstanceName);

            column = tableReference.ColumnInstances.Single(c => c.Name == columnName);
            columnItem = completionModel.Items.Single(i => i.InsertionText == columnName);
            columnMarkup = SymbolMarkup.ForSymbol(column);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsColumns_InGlobalContext()
        {
            var query = @"
                SELECT  |
                FROM    Employees e
            ";

            AssertIsMatch(query, "e", "EmployeeID");
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsColumns_AfterDot()
        {
            var query = @"
                SELECT  e.|
                FROM    Employees e
            ";

            AssertIsMatch(query, "e", "EmployeeID");
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsColumns_AfterText()
        {
            var query = @"
                SELECT  e.First|
                FROM    Employees e
            ";

            AssertIsMatch(query, "e", "FirstName");
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsAmbiguous()
        {
            var query = @"
                SELECT  |
                FROM    Employees e
                            INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsAmbiguousMatch(query, "e", "EmployeeID");
        }
    }
}