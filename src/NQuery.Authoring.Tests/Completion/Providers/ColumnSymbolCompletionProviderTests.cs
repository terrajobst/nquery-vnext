using NQuery.Authoring.Completion;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.Tests.Completion.Providers
{
    public class ColumnSymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, string tableInstanceName, string columnName)
        {
            GetCompletionData(query, tableInstanceName, columnName, out var column, out var columnItem, out var columnMarkup);

            Assert.Equal(Glyph.Column, columnItem.Glyph);
            Assert.Equal(column.Name, columnItem.DisplayText);
            Assert.Equal(columnMarkup.ToString(), columnItem.Description);
            Assert.Equal(column, columnItem.Symbol);
        }

        private static void AssertIsAmbiguousMatch(string query, string tableInstanceName, string columnName)
        {
            GetCompletionData(query, tableInstanceName, columnName, out var column, out var columnItem, out var columnMarkup);

            Assert.Equal(Glyph.AmbiguousName, columnItem.Glyph);
            Assert.Equal(column.Name, columnItem.DisplayText);
            Assert.StartsWith("Ambiguous Name:", columnItem.Description);
            Assert.Null(columnItem.Symbol);
        }

        private static void GetCompletionData(string query, string tableInstanceName, string columnName, out TableColumnInstanceSymbol column, out CompletionItem columnItem, out SymbolMarkup columnMarkup)
        {
            var completionModel = GetCompletionModel(query);
            var semanticModel = completionModel.SemanticModel;
            var syntaxTree = semanticModel.SyntaxTree;

            var tableReference = syntaxTree.Root.DescendantNodesAndSelf()
                .OfType<NamedTableReferenceSyntax>()
                .Select(semanticModel.GetDeclaredSymbol)
                .Single(s => s is not null && s.Name == tableInstanceName);

            column = tableReference.ColumnInstances.Single(c => c.Name == columnName);
            columnItem = completionModel.Items.Single(i => i.InsertionText == columnName);
            columnMarkup = SymbolMarkup.ForSymbol(column);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsColumns_InGlobalContext()
        {
            var query = @"
                SELECT  |
                FROM    Employees e
            ";

            AssertIsMatch(query, "e", "EmployeeID");
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsColumns_AfterDot()
        {
            var query = @"
                SELECT  e.|
                FROM    Employees e
            ";

            AssertIsMatch(query, "e", "EmployeeID");
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsColumns_AfterText()
        {
            var query = @"
                SELECT  e.First|
                FROM    Employees e
            ";

            AssertIsMatch(query, "e", "FirstName");
        }

        [Fact]
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