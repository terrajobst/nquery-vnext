using NQuery.Authoring.Selection;
using NQuery.Authoring.Selection.Providers;

namespace NQuery.Authoring.Tests.Selection.Providers
{
    public class GroupByClauseSelectionSpanProviderTests : SelectionSpanProviderTests
    {
        protected override ISelectionSpanProvider CreateProvider()
        {
            return new GroupByClauseSelectionSpanProvider();
        }

        [Fact]
        public void SelectionExtensions_GroupByClause()
        {
            var query = @"
                SELECT  e.Country,
                        e.City,
                        COUNT(*)
                FROM    Employees e
                GROUP   BY {{{e.Country},} e.City}
            ";

            AssertIsMatch(query);
        }
    }
}