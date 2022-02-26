using NQuery.Authoring.Selection;
using NQuery.Authoring.Selection.Providers;

namespace NQuery.Authoring.Tests.Selection.Providers
{
    public class OrderedQuerySelectionSpanProviderTests : SelectionSpanProviderTests
    {
        protected override ISelectionSpanProvider CreateProvider()
        {
            return new OrderedQuerySelectionSpanProvider();
        }

        [Fact]
        public void SelectionExtensions_OrderedQuery()
        {
            var query = @"
                SELECT  e.Country,
                        e.City
                FROM    Employees e
                ORDER   BY {{{e.Country DESC},} e.City}
            ";

            AssertIsMatch(query);
        }
    }
}