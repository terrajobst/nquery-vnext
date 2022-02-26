using System.Collections.Immutable;

using NQuery.Authoring.CodeActions;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions
{
    public abstract class CodeActionTest
    {
        protected abstract ImmutableArray<ICodeAction> GetActions(string query);

        protected void AssertDoesNotTrigger(string query)
        {
            var actions = GetActions(query);

            Assert.Empty(actions);
        }

        protected void AssertDoesNotTrigger(string query, string expectedActionDescription)
        {
            var actions = GetActions(query).Where(a => a.Description == expectedActionDescription).ToImmutableArray();

            Assert.Empty(actions);
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