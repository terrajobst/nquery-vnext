using System;
using System.Linq;

using NQuery.Symbols;

using Xunit;

namespace NQuery.Authoring.Tests.Completion.Providers
{
    public class PropertySymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, Type type, string propertyName)
        {
            var completionModel = GetCompletionModel(query);
            var semanticModel = completionModel.SemanticModel;

            var property = semanticModel.LookupProperties(type).Single(p => p.Name == propertyName);
            var propertyItem = completionModel.Items.Single(i => i.InsertionText == property.Name);
            var propertyMarkup = SymbolMarkup.ForSymbol(property);

            Assert.Equal(Glyph.Property, propertyItem.Glyph);
            Assert.Equal(property.Name, propertyItem.DisplayText);
            Assert.Equal(propertyMarkup.ToString(), propertyItem.Description);
            Assert.Equal(property, propertyItem.Symbol);
        }

        private static void AssertIsNoMatch(string query)
        {
            var completionModel = GetCompletionModel(query);
            var hasTables = completionModel.Items.Any(i => i.Symbol is PropertySymbol || i.Glyph == Glyph.Property);
            Assert.False(hasTables);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsProperties_AfterDot()
        {
            var query = @"
                SELECT  e.FirstName.|
                FROM    Employees e
            ";

            AssertIsMatch(query, typeof(string), "Length");
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsProperties_AfterText()
        {
            var query = @"
                SELECT  e.FirstName.Len|
                FROM    Employees e
            ";

            AssertIsMatch(query, typeof(string), "Length");
        }

        [Fact]
        public void SymbolCompletionProvider_DoesNotReturnProperties_ForTableInstance()
        {
            var query = @"
                SELECT  e.|
                FROM    Employees e
            ";

            AssertIsNoMatch(query);
        }
    }
}