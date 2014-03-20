using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Symbols;

namespace NQuery.Authoring.UnitTests.Completion.Providers
{
    [TestClass]
    public class TableSymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, string tableName)
        {
            var completionModel = GetCompletionModel(query);
            var dataContext = completionModel.SemanticModel.Compilation.DataContext;

            var table = dataContext.Tables.Single(t => t.Name == tableName);
            var tableItem = completionModel.Items.Single(i => i.InsertionText == table.Name);
            var tableMarkup = SymbolMarkup.ForSymbol(table);

            Assert.AreEqual(NQueryGlyph.Table, tableItem.Glyph);
            Assert.AreEqual(table.Name, tableItem.DisplayText);
            Assert.AreEqual(tableMarkup.ToString(), tableItem.Description);
            Assert.AreEqual(table, tableItem.Symbol);
        }

        private static void AssertIsNoMatch(string query)
        {
            var completionModel = GetCompletionModel(query);
            var hasTables = completionModel.Items.Any(i => i.Symbol is TableSymbol || i.Glyph == NQueryGlyph.Table);
            Assert.IsFalse(hasTables);
        }

        private static void AssertReturnsOnlyTables(string query)
        {
            var completionModel = GetCompletionModel(query);
            var hasNonTables = completionModel.Items.Any(i => !(i.Symbol is TableSymbol) || i.Glyph != NQueryGlyph.Table);
            Assert.IsFalse(hasNonTables);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsTables_AfterFrom()
        {
            var query = @"
                SELECT  *
                FROM    |
            ";

            AssertIsMatch(query, "Employees");
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsTables_AfterOnKeyword()
        {
            var query = @"
                SELECT  *
                FROM    Or|
            ";

            AssertIsMatch(query, "Orders");
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsTables_InJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER JOIN Emp|
            ";

            AssertIsMatch(query, "EmployeeTerritories");
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsOnlyTables_InTableContext()
        {
            var query = @"
                SELECT  *
                FROM    |
            ";

            AssertReturnsOnlyTables(query);
        }

        [TestMethod]
        public void SymbolCompletionProvider_DoesNotReturnTables_InGlobalContext()
        {
            var query = @"
                SELECT  |
                FROM    Employees e
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void SymbolCompletionProvider_DoesNotReturnTables_InJoinCondition()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER JOIN EmployeeTerritories et ON |
            ";

            AssertIsNoMatch(query);
        }
    }
}