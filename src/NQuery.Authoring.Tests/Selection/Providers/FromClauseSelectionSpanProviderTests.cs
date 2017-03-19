using NQuery.Authoring.Selection;
using NQuery.Authoring.Selection.Providers;
using Xunit;

namespace NQuery.Authoring.Tests.Selection.Providers
{
    public class FromClauseSelectionSpanProviderTests : SelectionSpanProviderTests
    {
        protected override ISelectionSpanProvider CreateProvider()
        {
            return new FromClauseSelectionSpanProvider();
        }

        [Fact]
        public void SelectionExtensions_FromClause()
        {
            var query = @"
                SELECT  *
                FROM    {{{Employees e},}
                        EmployeeTerritories et,
                        Territories t}
            ";

            AssertIsMatch(query);
        }
    }
}