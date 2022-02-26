using NQuery.Authoring.Selection;
using NQuery.Authoring.Selection.Providers;

namespace NQuery.Authoring.Tests.Selection.Providers
{
    public class CommonTableExpressionQuerySelectionSpanProviderTests : SelectionSpanProviderTests
    {
        protected override ISelectionSpanProvider CreateProvider()
        {
            return new CommonTableExpressionQuerySelectionSpanProvider();
        }

        [Fact]
        public void SelectionExtensions_CommonTableExpressionQuery()
        {
            var query = @"
                WITH {{{LondonEmps AS
                (
                    SELECT  *
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )},}
                SeattleEmps AS
                (
                    SELECT  *
                    FROM    Employees e
                    WHERE   e.City = 'Seattle'
                )}
                SELECT  *
                FROM    Employees
            ";

            AssertIsMatch(query);
        }
    }
}