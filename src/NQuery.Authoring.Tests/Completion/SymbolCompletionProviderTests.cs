using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;

namespace NQuery.Authoring.Tests.Completion
{
    public abstract class SymbolCompletionProviderTests
    {
        protected static CompletionModel GetCompletionModel(string query)
        {
            var compilation = CompilationFactory.CreateQuery(query, out int position);
            var semanticModel = compilation.GetSemanticModel();

            var provider = new SymbolCompletionProvider();
            var providers = new[] { provider };

            return semanticModel.GetCompletionModel(position, providers);
        }
    }
}