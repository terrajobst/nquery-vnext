using NQuery.Authoring.Outlining;
using NQuery.Authoring.Outlining.Outliners;

using Xunit;

namespace NQuery.Authoring.Tests.Outlining.Outliners
{
    public class SelectQueryOutlinerTests : OutlinerTests
    {
        protected override IOutliner CreateOutliner()
        {
            return new SelectQueryOutliner();
        }

        [Fact]
        public void SelectQueryOutliner_FindsSelectQuery()
        {
            var query = @"
                {SELECT  *
                FROM    Employees e
                WHERE   e.City = 'London'}
            ";

            AssertIsMatch(query, "SELECT");
        }

        [Fact]
        public void SelectQueryOutliner_DoesNotTriggerForSingleLineQueries()
        {
            var query = @"
                SELECT  1, 2, 3
            ";

            AssertIsNoMatch(query);
        }

        [Fact]
        public void SelectQueryOutliner_DoesNotTriggerForOrderedQueries()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.City = 'London'
                ORDER   BY 1
            ";

            AssertIsNoMatch(query);
        }
    }
}