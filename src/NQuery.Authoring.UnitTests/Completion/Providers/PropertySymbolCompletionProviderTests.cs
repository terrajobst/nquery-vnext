using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Symbols;

namespace NQuery.Authoring.UnitTests.Completion.Providers
{
    [TestClass]
    public class PropertySymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsMatch(string query, Type type, string propertyName)
        {
            var completionModel = GetCompletionModel(query);
            var semanticModel = completionModel.SemanticModel;

            var property = semanticModel.LookupProperties(type).Single(p => p.Name == propertyName);
            var propertyItem = completionModel.Items.Single(i => i.InsertionText == property.Name);
            var propertyMarkup = SymbolMarkup.ForSymbol(property);

            Assert.AreEqual(NQueryGlyph.Property, propertyItem.Glyph);
            Assert.AreEqual(property.Name, propertyItem.DisplayText);
            Assert.AreEqual(propertyMarkup.ToString(), propertyItem.Description);
            Assert.AreEqual(property, propertyItem.Symbol);
        }

        private static void AssertIsNoMatch(string query)
        {
            var completionModel = GetCompletionModel(query);
            var hasTables = completionModel.Items.Any(i => i.Symbol is PropertySymbol || i.Glyph == NQueryGlyph.Property);
            Assert.IsFalse(hasTables);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsProperties_AfterDot()
        {
            var query = @"
                SELECT  e.FirstName.|
                FROM    Employees e
            ";

            AssertIsMatch(query, typeof(string), "Length");
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsProperties_AfterText()
        {
            var query = @"
                SELECT  e.FirstName.Len|
                FROM    Employees e
            ";

            AssertIsMatch(query, typeof(string), "Length");
        }

        [TestMethod]
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