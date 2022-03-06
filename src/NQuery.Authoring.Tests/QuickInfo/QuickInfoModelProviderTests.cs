using NQuery.Authoring.QuickInfo;
using NQuery.Text;

namespace NQuery.Authoring.Tests.QuickInfo
{
    public abstract class QuickInfoModelProviderTests
    {
        protected abstract IQuickInfoModelProvider CreateProvider();

        protected abstract QuickInfoModel CreateExpectedModel(SemanticModel semanticModel);

        protected void AssertIsMatch(string query)
        {
            AssertIsMatch(query, null);
        }

        protected void AssertIsMatch(string query, Func<DataContext, DataContext> dataContextModifer)
        {
            GetModels(query, dataContextModifer, out var semanticModel, out var startModel, out var middleModel, out var endModel);

            var expectedModel = CreateExpectedModel(semanticModel);

            AssertIsMatch(expectedModel, startModel);
            AssertIsMatch(expectedModel, middleModel);
            AssertIsMatch(expectedModel, endModel);
        }

        protected void AssertIsNotMatch(string query)
        {
            AssertIsNotMatch(query, null);
        }

        protected void AssertIsNotMatch(string query, Func<DataContext, DataContext> dataContextModifer)
        {
            GetModels(query, dataContextModifer, out _, out var startModel, out var middleModel, out var endModel);

            Assert.Null(startModel);
            Assert.Null(middleModel);
            Assert.Null(endModel);
        }

        private static void AssertIsMatch(QuickInfoModel expectedModel, QuickInfoModel actualModel)
        {
            Assert.NotNull(actualModel);

            Assert.Equal(expectedModel.SemanticModel, actualModel.SemanticModel);
            Assert.Equal(expectedModel.Span, actualModel.Span);
            Assert.Equal(expectedModel.Glyph, actualModel.Glyph);
            Assert.Equal(expectedModel.Markup, actualModel.Markup);
        }

        private void GetModels(string query, Func<DataContext, DataContext> dataContextModifer, out SemanticModel semanticModel, out QuickInfoModel startModel, out QuickInfoModel middleModel, out QuickInfoModel endModel)
        {
            var compilation = CompilationFactory.CreateQuery(query, out TextSpan span);

            if (dataContextModifer is not null)
                compilation = compilation.WithDataContext(dataContextModifer(compilation.DataContext));

            semanticModel = compilation.GetSemanticModel();
            var start = span.Start;
            var middle = span.Start + span.Length / 2;
            var end = span.End;

            var provider = CreateProvider();
            var providers = new[] { provider };

            startModel = semanticModel.GetQuickInfoModel(start, providers);
            middleModel = semanticModel.GetQuickInfoModel(middle, providers);
            endModel = semanticModel.GetQuickInfoModel(end, providers);
        }
    }
}