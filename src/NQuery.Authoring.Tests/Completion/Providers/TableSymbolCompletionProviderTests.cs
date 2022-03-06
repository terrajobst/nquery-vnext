using NQuery.Symbols;

namespace NQuery.Authoring.Tests.Completion.Providers
{
    public class TableSymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, string tableName)
        {
            var completionModel = GetCompletionModel(query);
            var dataContext = completionModel.SemanticModel.Compilation.DataContext;

            var table = dataContext.Tables.Single(t => t.Name == tableName);
            var tableItem = completionModel.Items.Single(i => i.InsertionText == table.Name);
            var tableMarkup = SymbolMarkup.ForSymbol(table);

            Assert.Equal(Glyph.Table, tableItem.Glyph);
            Assert.Equal(table.Name, tableItem.DisplayText);
            Assert.Equal(tableMarkup.ToString(), tableItem.Description);
            Assert.Equal(table, tableItem.Symbol);
        }

        private static void AssertIsNoMatch(string query)
        {
            var completionModel = GetCompletionModel(query);
            var hasTables = completionModel.Items.Any(i => i.Symbol is TableSymbol || i.Glyph == Glyph.Table);
            Assert.False(hasTables);
        }

        private static void AssertReturnsOnlyTables(string query)
        {
            var completionModel = GetCompletionModel(query);
            var hasNonTables = completionModel.Items.Any(i => i.Symbol is not TableSymbol || i.Glyph != Glyph.Table);
            Assert.False(hasNonTables);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsTables_AfterFrom()
        {
            var query = @"
                SELECT  *
                FROM    |
            ";

            AssertIsMatch(query, "Employees");
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsTables_AfterOnKeyword()
        {
            var query = @"
                SELECT  *
                FROM    Or|
            ";

            AssertIsMatch(query, "Orders");
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsTables_InJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER JOIN Emp|
            ";

            AssertIsMatch(query, "EmployeeTerritories");
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsOnlyTables_InTableContext()
        {
            var query = @"
                SELECT  *
                FROM    |
            ";

            AssertReturnsOnlyTables(query);
        }

        [Fact]
        public void SymbolCompletionProvider_DoesNotReturnTables_InGlobalContext()
        {
            var query = @"
                SELECT  |
                FROM    Employees e
            ";

            AssertIsNoMatch(query);
        }

        [Fact]
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