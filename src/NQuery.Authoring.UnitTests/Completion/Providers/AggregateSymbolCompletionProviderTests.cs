using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Symbols;

namespace NQuery.Authoring.UnitTests.Completion.Providers
{
    [TestClass]
    public class AggregateSymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, string aggregateName)
        {
            var completionModel = GetCompletionModel(query);
            var dataContext = completionModel.SemanticModel.Compilation.DataContext;

            var function = dataContext.Aggregates.Single(t => t.Name == aggregateName);
            var functionItem = completionModel.Items.Single(i => i.InsertionText == function.Name);
            var functionMarkup = SymbolMarkup.ForSymbol(function);

            Assert.AreEqual(NQueryGlyph.Aggregate, functionItem.Glyph);
            Assert.AreEqual(function.Name, functionItem.DisplayText);
            Assert.AreEqual(functionMarkup.ToString(), functionItem.Description);
            Assert.AreEqual(function, functionItem.Symbol);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsAggregates_InGlobalContext()
        {
            var query = @"
                SELECT  |
                FROM    Employees
            ";

            AssertIsMatch(query, "COUNT");
        }

        [TestMethod]
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