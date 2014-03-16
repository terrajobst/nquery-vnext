using System;
using System.Linq;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.UnitTests.CodeActions
{
    public abstract class CodeIssueTests
    {
        protected CodeIssue[] GetIssues(string query)
        {
            var compilation = CompilationFactory.CreateQuery(query);
            var semanticModel = compilation.GetSemanticModel();

            var provider = CreateProvider();
            var providers = new[] {provider};
            return semanticModel.GetIssues(providers).ToArray();
        }

        protected abstract ICodeIssueProvider CreateProvider();
    }
}