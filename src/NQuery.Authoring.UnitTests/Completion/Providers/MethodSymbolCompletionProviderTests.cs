using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Symbols;

namespace NQuery.Authoring.UnitTests.Completion.Providers
{
    [TestClass]
    public class MethodSymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, Type type, string methodName)
        {
            var completionModel = GetCompletionModel(query);
            var semanticModel = completionModel.SemanticModel;

            var method = semanticModel.LookupMethods(type)
                                      .Where(m => m.Name == methodName)
                                      .OrderBy(m => m.Parameters.Count)
                                      .First();
            var methodItem = completionModel.Items.Single(i => i.InsertionText == method.Name);
            var methodMarkup = SymbolMarkup.ForSymbol(method);

            var overloadCount = semanticModel.LookupMethods(type)
                                             .Count(m => m.Name == methodName) - 1;
            var expectedDescription = overloadCount == 0
                ? methodMarkup.ToString()
                : string.Format("{0} (+ {1} overload(s))", methodMarkup, overloadCount);

            Assert.AreEqual(NQueryGlyph.Method, methodItem.Glyph);
            Assert.AreEqual(method.Name, methodItem.DisplayText);
            Assert.AreEqual(expectedDescription, methodItem.Description);
        }

        private static void AssertIsNoMatch(string query)
        {
            var completionModel = GetCompletionModel(query);
            var hasTables = completionModel.Items.Any(i => i.Symbol is MethodSymbol || i.Glyph == NQueryGlyph.Method);
            Assert.IsFalse(hasTables);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsMethods_AfterDot()
        {
            var query = @"
                SELECT  e.FirstName.|
                FROM    Employees e
            ";

            AssertIsMatch(query, typeof(string), "Contains");
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsMethods_AfterText()
        {
            var query = @"
                SELECT  e.FirstName.Sub|
                FROM    Employees e
            ";

            AssertIsMatch(query, typeof(string), "Substring");
        }

        [TestMethod]
        public void SymbolCompletionProvider_DoesNotReturnMethods_ForTableInstance()
        {
            var query = @"
                SELECT  e.|
                FROM    Employees e
            ";

            AssertIsNoMatch(query);
        }
    }
}