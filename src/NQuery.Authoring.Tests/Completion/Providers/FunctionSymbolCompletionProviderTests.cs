using System;
using System.Linq;

using Xunit;

using NQuery.Symbols;

namespace NQuery.Authoring.UnitTests.Completion.Providers
{
    public class FunctionSymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, string functionName)
        {
            var completionModel = GetCompletionModel(query);
            var dataContext = completionModel.SemanticModel.Compilation.DataContext;

            var function = dataContext.Functions.Single(t => t.Name == functionName);
            var functionItem = completionModel.Items.Single(i => i.InsertionText == function.Name);
            var functionMarkup = SymbolMarkup.ForSymbol(function);

            Assert.Equal(NQueryGlyph.Function, functionItem.Glyph);
            Assert.Equal(function.Name, functionItem.DisplayText);
            Assert.Equal(functionMarkup.ToString(), functionItem.Description);
            Assert.Equal(function, functionItem.Symbol);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsFunctions_InGlobalContext()
        {
            var query = @"
                SELECT  |
                FROM    Employees
            ";

            AssertIsMatch(query, "SIN");
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsFunctions_AfterText()
        {
            var query = @"
                SELECT  S|
                FROM    Employees
            ";

            AssertIsMatch(query, "SIN");
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsFunctions_AfterContextualKeyword()
        {
            var query = @"
                SELECT  Left|
                FROM    Employees
            ";

            AssertIsMatch(query, "LEFT");
        }
    }
}