using NQuery.Authoring.Selection;
using NQuery.Authoring.Selection.Providers;
using Xunit;

namespace NQuery.Authoring.Tests.Selection.Providers
{
    public class CommonTableExpressionColumnNameListSelectionSpanProviderTests : SelectionSpanProviderTests
    {
        protected override ISelectionSpanProvider CreateProvider()
        {
            return new CommonTableExpressionColumnNameListSelectionSpanProvider();
        }

        [Fact]
        public void SelectionExtensions_CommonTableExpressionColumnNameList()
        {
            var query = @"
                WITH LondonEmps ({{{FN},} LN, RC}) AS
                (
                    SELECT  e.FirstName,
                            e.LastName,
                            e.RegionCode
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  *
                FROM    LondonEmps
            ";

            AssertIsMatch(query);
        }
    }
}