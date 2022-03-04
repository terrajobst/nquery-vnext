using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;

namespace NQuery.Authoring.Tests.Completion
{
    public abstract class SymbolCompletionProviderTests
    {
        protected static CompletionModel GetCompletionModel(string query)
        {
            int position;
            var compilation = CompilationFactory.CreateQuery(query, out position);
            var semanticModel = compilation.GetSemanticModel();

            var provider = new SymbolCompletionProvider();
            var providers = new[] { provider };

            return semanticModel.GetCompletionModel(position, providers);
        }
    }
}