using NQuery.Authoring.Outlining;
using NQuery.Authoring.Outlining.Outliners;

namespace NQuery.Authoring.Tests.Outlining.Outliners
{
    public class MultiLineCommentOutlinerTests : OutlinerTests
    {
        protected override IOutliner CreateOutliner()
        {
            return new MultiLineCommentOutliner();
        }

        [Fact]
        public void MultiLineCommentOutliner_FindsComments_IfLeading()
        {
            var query = @"
                {/*
                 * This is a query.
                 */}
                SELECT  FirstName,
                        LastName
                FROM    Employees
            ";

            AssertIsMatch(query, "/* ...");
        }

        [Fact]
        public void MultiLineCommentOutliner_FindsComments_IfTrailing()
        {
            var query = @"
                SELECT  FirstName, {/* First
                                      Column */}
                        LastName
                FROM    Employees
            ";

            AssertIsMatch(query, "/* First ...");
        }

        [Fact]
        public void MultiLineCommentOutliner_FindsComments_AtEof()
        {
            var query = @"
                SELECT  FirstName,
                        LastName
                FROM    Employees
                {/*
                  End of query
                */}
            ";

            AssertIsMatch(query, "/* ...");
        }

        [Fact]
        public void MultiLineCommentOutliner_DoesNotTriggerForSingeLines()
        {
            var query = @"
                /* The Query */
                SELECT  FirstName, /* First Column */
                        LastName   /* Second Column */
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }
    }
}