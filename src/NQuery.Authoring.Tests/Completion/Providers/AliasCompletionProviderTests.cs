using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;

namespace NQuery.Authoring.Tests.Completion.Providers
{
    public class AliasCompletionProviderTests
    {
        private static void AssertIsMatch(string queryWithPosition)
        {
            var completionModel = GetCompletionModel(queryWithPosition);
            var item = completionModel.Items.Single();

            Assert.Null(item.Glyph);
            Assert.Null(item.Description);
            Assert.Equal("a", item.DisplayText);
            Assert.Equal("a", item.InsertionText);
            Assert.Null(item.Symbol);
            Assert.True(item.IsBuilder);
        }

        private static void AssertIsNoMatch(string queryWithPosition)
        {
            var completionModel = GetCompletionModel(queryWithPosition);

            Assert.Empty(completionModel.Items);
        }

        private static CompletionModel GetCompletionModel(string queryWithPosition)
        {
            var normalized = queryWithPosition.NormalizeCode();

            var query = normalized.ParseSinglePosition(out var position);

            var compilation = CompilationFactory.CreateQuery(query);
            var semanticModel = compilation.GetSemanticModel();

            var provider = new AliasCompletionProvider();
            var providers = new[] { provider };

            var completionModel = semanticModel.GetCompletionModel(position, providers);
            return completionModel;
        }

        [Fact]
        public void AliasCompletionProvider_ReturnsBuilder_WhenValidPrefixIsPresent()
        {
            var query = @"
                SELECT  *
                FROM    Employees a|
            ";

            AssertIsMatch(query);
        }

        [Fact]
        public void AliasCompletionProvider_ReturnsNoBuilder_WhenAsIsMissing()
        {
            var query = @"
                SELECT  *
                FROM    Employees |
            ";

            AssertIsNoMatch(query);
        }

        [Fact]
        public void AliasCompletionProvider_ReturnsNoBuilder_WhenAsIsPresent()
        {
            var query = @"
                SELECT  *
                FROM    Employees as|
            ";

            AssertIsNoMatch(query);
        }
    }
}