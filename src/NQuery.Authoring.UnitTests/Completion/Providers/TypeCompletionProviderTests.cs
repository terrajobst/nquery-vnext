using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;

namespace NQuery.Authoring.UnitTests.Completion.Providers
{
    [TestClass]
    public class TypeCompletionProviderTests
    {
        private static CompletionModel GetCompletionModel(string queryWithJoinMarker)
        {
            var normalized = queryWithJoinMarker.NormalizeCode();

            int position;
            var compilation = CompilationFactory.CreateQuery(normalized, out position);
            var semanticModel = compilation.GetSemanticModel();

            var provider = new TypeCompletionProvider();
            var providers = new[] {provider};

            return semanticModel.GetCompletionModel(position, providers);
        }

        private static void AssertIsMatch(string query)
        {
            var completionModel = GetCompletionModel(query);
            var items = completionModel.Items.ToDictionary(i => i.InsertionText);
            var typeNames = SyntaxFacts.GetTypeNames();

            foreach (var typeName in typeNames)
            {
                var item = items[typeName];
                Assert.AreEqual(NQueryGlyph.Type, item.Glyph);
                Assert.AreEqual(typeName, item.Description);
                Assert.AreEqual(typeName, item.DisplayText);
                Assert.AreEqual(typeName, item.InsertionText);
                Assert.AreEqual(null, item.Symbol);
            }
        }

        private static void AssertIsNotMatch(string query)
        {
            var completionModel = GetCompletionModel(query);
            var items = completionModel.Items.ToDictionary(i => i.InsertionText);
            var typeNames = SyntaxFacts.GetTypeNames();

            var returnsAnyTypes = typeNames.Any(items.ContainsKey);
            Assert.IsFalse(returnsAnyTypes);
        }

        [TestMethod]
        public void TypeCompletionProvider_ReturnsTypes()
        {
            var query = @"
                SELECT  CAST(1 AS |
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void TypeCompletionProvider_DoesNotReturnTypes_IfNoAs()
        {
            var query = @"
                SELECT  CAST(1 |
            ";

            AssertIsNotMatch(query);
        }
    }
}