using System;

using NQuery.Authoring.Outlining;
using NQuery.Authoring.Outlining.Outliners;

using Xunit;

namespace NQuery.Authoring.Tests.Outlining.Outliners
{
    public class SingleLineCommentOutlinerTests : OutlinerTests
    {
        protected override IOutliner CreateOutliner()
        {
            return new SingleLineCommentOutliner();
        }

        [Fact]
        public void SingleLineCommentOutliner_DoesNotTriggerForSingeLines()
        {
            var query = @"
                // The Query
                SELECT  FirstName, -- First Column
                        LastName   // Second Column
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [Fact]
        public void SingleLineCommentOutliner_FindsConsecutive()
        {
            var query = @"
                {//---------------------
                // This is a query.
                //---------------------}
                SELECT  FirstName,
                        LastName
                FROM    Employees
            ";

            AssertIsMatch(query, "//--------------------- ...");
        }

        [Fact]
        public void SingleLineCommentOutliner_FindsConsecutive_WhenSlashSlashAndMinusMinusAreMixed()
        {
            var query = @"
                {-----------------------
                // This is a query.
                -----------------------}
                SELECT  FirstName,
                        LastName
                FROM    Employees
            ";

            AssertIsMatch(query, "----------------------- ...");
        }

        [Fact]
        public void SingleLineCommentOutliner_FindsConsecutive_ButDoesNotCombineAcrossTokens()
        {
            var query = @"
                SELECT  FirstName, // First column
                        {// Second
                        // Column}
                        LastName
                FROM    Employees
            ";

            AssertIsMatch(query, "// Second ...");
        }

        [Fact]
        public void SingleLineCommentOutliner_DoesNotCombineLeadingAndTrailing()
        {
            var query = @"
                // First line
                SELECT  // Second line
                        FirstName
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [Fact]
        public void SingleLineCommentOutliner_DoesNotCombineSingleAndMultiLineComments()
        {
            var query = @"
                // First line
                /* Second line */
                {// Third line
                // Fourth line}
                SELECT  FirstName
                FROM    Employees
            ";

            AssertIsMatch(query, "// Third line ...");
        }
    }
}