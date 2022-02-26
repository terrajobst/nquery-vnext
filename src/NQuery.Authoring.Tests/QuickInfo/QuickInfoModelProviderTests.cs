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
            SemanticModel semanticModel;
            QuickInfoModel startModel;
            QuickInfoModel middleModel;
            QuickInfoModel endModel;
            GetModels(query, dataContextModifer, out semanticModel, out startModel, out middleModel, out endModel);

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
            SemanticModel semanticModel;
            QuickInfoModel startModel;
            QuickInfoModel middleModel;
            QuickInfoModel endModel;
            GetModels(query, dataContextModifer, out semanticModel, out startModel, out middleModel, out endModel);

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
            TextSpan span;
            var compilation = CompilationFactory.CreateQuery(query, out span);

            if (dataContextModifer != null)
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