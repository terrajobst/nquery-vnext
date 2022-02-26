using NQuery.Authoring.Outlining;
using NQuery.Authoring.Outlining.Outliners;

namespace NQuery.Authoring.Tests.Outlining.Outliners
{
    public class OrderedQueryOutlinerTests : OutlinerTests
    {
        protected override IOutliner CreateOutliner()
        {
            return new OrderedQueryOutliner();
        }

        [Fact]
        public void OrderedQueryOutliner_FindsSelectWithOrderByQuery()
        {
            var query = @"
                {SELECT  *
                FROM    Employees e
                WHERE   e.City = 'London'
                ORDER   BY 1}
            ";

            AssertIsMatch(query, "SELECT");
        }

        [Fact]
        public void OrderedQueryOutliner_FindsNonSelectQueries()
        {
            var query = @"
                {(
                    SELECT  *
                    FROM    Employees e
                    WHERE   e.City = 'London'

                    UNION   ALL

                    SELECT  *
                    FROM    Employees
                )
                ORDER   BY 1}
            ";

            AssertIsMatch(query, "...");
        }

        [Fact]
        public void OrderedQueryOutliner_DoesNotTriggerSingleLineQueries()
        {
            var query = @"
                SELECT  * FROM Employees ORDER BY 1
            ";

            AssertIsNoMatch(query);
        }

        [Fact]
        public void OrderedQueryOutliner_DoesNotTriggerForUnorderedQueries()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.City = 'London'
            ";

            AssertIsNoMatch(query);
        }
    }
}