using System.Collections.Immutable;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Tests.CodeActions
{
    public abstract class CodeIssueTests
    {
        protected ImmutableArray<CodeIssue> GetIssues(string query)
        {
            var compilation = CompilationFactory.CreateQuery(query);
            var semanticModel = compilation.GetSemanticModel();

            var provider = CreateProvider();
            var providers = new[] { provider };
            return semanticModel.GetIssues(providers).ToImmutableArray();
        }

        protected abstract ICodeIssueProvider CreateProvider();
    }
}