using System.Collections.Immutable;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Tests.CodeActions
{
    public abstract class CodeFixTests : CodeActionTest
    {
        protected override ImmutableArray<ICodeAction> GetActions(string query)
        {
            int position;
            var compilation = CompilationFactory.CreateQuery(query, out position);
            var semanticModel = compilation.GetSemanticModel();

            var provider = CreateProvider();
            var providers = new[] { provider };
            return semanticModel.GetFixes(position, providers).ToImmutableArray();
        }

        protected abstract ICodeFixProvider CreateProvider();
    }
}