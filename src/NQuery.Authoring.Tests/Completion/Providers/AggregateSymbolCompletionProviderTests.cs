using System;
using System.Linq;

using NQuery.Symbols;

using Xunit;

namespace NQuery.Authoring.Tests.Completion.Providers
{
    public class AggregateSymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, string aggregateName)
        {
            var completionModel = GetCompletionModel(query);
            var dataContext = completionModel.SemanticModel.Compilation.DataContext;

            var function = dataContext.Aggregates.Single(t => t.Name == aggregateName);
            var functionItem = completionModel.Items.Single(i => i.InsertionText == function.Name);
            var functionMarkup = SymbolMarkup.ForSymbol(function);

            Assert.Equal(Glyph.Aggregate, functionItem.Glyph);
            Assert.Equal(function.Name, functionItem.DisplayText);
            Assert.Equal(functionMarkup.ToString(), functionItem.Description);
            Assert.Equal(function, functionItem.Symbol);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsAggregates_InGlobalContext()
        {
            var query = @"
                SELECT  |
                FROM    Employees
            ";

            AssertIsMatch(query, "COUNT");
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsAggregates_AfterText()
        {
            var query = @"
                SELECT  S|
                FROM    Employees
            ";

            AssertIsMatch(query, "SUM");
        }
    }
}