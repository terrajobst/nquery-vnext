using System;
using System.Collections.Immutable;
using System.Linq;

using Xunit;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.UnitTests.CodeActions
{
    public abstract class RefactoringTests
    {
        protected ImmutableArray<ICodeAction> GetActions(string query)
        {
            int position;
            var compilation = CompilationFactory.CreateQuery(query, out position);
            var semanticModel = compilation.GetSemanticModel();

            var provider = CreateProvider();
            var providers = new[] {provider};
            return semanticModel.GetRefactorings(position, providers).ToImmutableArray();
        }

        protected abstract ICodeRefactoringProvider CreateProvider();

        protected void AssertDoesNotTrigger(string query)
        {
            var actions = GetActions(query);

            Assert.Equal(0, actions.Length);
        }

        protected void AssertDoesNotTrigger(string query, string expectedActionDescription)
        {
            var actions = GetActions(query).Where(a => a.Description == expectedActionDescription).ToImmutableArray();

            Assert.Equal(0, actions.Length);
        }

        protected void AssertFixes(string query, string expectedFixedQuery, string expectedActionDescription)
        {
            var trimmedQuery = query.NormalizeCode();
            var trimmedExpectedQuery = expectedFixedQuery.NormalizeCode();

            var actions = GetActions(trimmedQuery);
            var action = actions.Single(a => a.Description == expectedActionDescription);
            var syntaxTree = action.GetEdit();
            Assert.Equal(trimmedExpectedQuery, syntaxTree.Text.GetText());
        }
    }
}