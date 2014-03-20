using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Symbols;

namespace NQuery.Authoring.UnitTests.Completion.Providers
{
    [TestClass]
    public class FunctionSymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, string functionName)
        {
            var completionModel = GetCompletionModel(query);
            var dataContext = completionModel.SemanticModel.Compilation.DataContext;

            var function = dataContext.Functions.Single(t => t.Name == functionName);
            var functionItem = completionModel.Items.Single(i => i.InsertionText == function.Name);
            var functionMarkup = SymbolMarkup.ForSymbol(function);

            Assert.AreEqual(NQueryGlyph.Function, functionItem.Glyph);
            Assert.AreEqual(function.Name, functionItem.DisplayText);
            Assert.AreEqual(functionMarkup.ToString(), functionItem.Description);
            Assert.AreEqual(function, functionItem.Symbol);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsFunctions_InGlobalContext()
        {
            var query = @"
                SELECT  |
                FROM    Employees
            ";

            AssertIsMatch(query, "SIN");
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsFunctions_AfterText()
        {
            var query = @"
                SELECT  S|
                FROM    Employees
            ";

            AssertIsMatch(query, "SIN");
        }

        [TestMethod]
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