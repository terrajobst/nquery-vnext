using System;
using System.Linq;

using NQuery.Symbols;

using Xunit;

namespace NQuery.Authoring.Tests.Completion.Providers
{
    public class MethodSymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, Type type, string methodName)
        {
            var completionModel = GetCompletionModel(query);
            var semanticModel = completionModel.SemanticModel;

            var method = semanticModel.LookupMethods(type)
                                      .Where(m => m.Name == methodName)
                                      .OrderBy(m => m.Parameters.Length)
                                      .First();
            var methodItem = completionModel.Items.Single(i => i.InsertionText == method.Name);
            var methodMarkup = SymbolMarkup.ForSymbol(method);

            var overloadCount = semanticModel.LookupMethods(type)
                                             .Count(m => m.Name == methodName) - 1;
            var expectedDescription = overloadCount == 0
                ? methodMarkup.ToString()
                : string.Format("{0} (+ {1} overload(s))", methodMarkup, overloadCount);

            Assert.Equal(Glyph.Method, methodItem.Glyph);
            Assert.Equal(method.Name, methodItem.DisplayText);
            Assert.Equal(expectedDescription, methodItem.Description);
        }

        private static void AssertIsNoMatch(string query)
        {
            var completionModel = GetCompletionModel(query);
            var hasTables = completionModel.Items.Any(i => i.Symbol is MethodSymbol || i.Glyph == Glyph.Method);
            Assert.False(hasTables);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsMethods_AfterDot()
        {
            var query = @"
                SELECT  e.FirstName.|
                FROM    Employees e
            ";

            AssertIsMatch(query, typeof(string), "Contains");
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsMethods_AfterText()
        {
            var query = @"
                SELECT  e.FirstName.Sub|
                FROM    Employees e
            ";

            AssertIsMatch(query, typeof(string), "Substring");
        }

        [Fact]
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