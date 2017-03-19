using NQuery.Authoring.Selection;
using NQuery.Authoring.Selection.Providers;
using Xunit;

namespace NQuery.Authoring.Tests.Selection.Providers
{
    public class SelectClauseSelectionSpanProviderTests : SelectionSpanProviderTests
    {
        protected override ISelectionSpanProvider CreateProvider()
        {
            return new SelectClauseSelectionSpanProvider();
        }

        [Fact]
        public void SelectionExtensions_SelectClause()
        {
            var query = @"
                SELECT  {{{e.Country},}
                        e.City}
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }
    }
}