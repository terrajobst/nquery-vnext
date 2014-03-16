using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class DateBraceMatcherTests : BraceMatcherTests
    {
        public DateBraceMatcherTests()
            : base("#")
        {
        }

        protected override IBraceMatcher CreateMatcher()
        {
            return new DateBraceMatcher();
        }

        [TestMethod]
        public void DateBraceMatcher_MatchesAtStartOfFirstHash()
        {
            var query = @"
                SELECT  |#10/10/1993#
                FROM    Employees
            ";

            AssertIsMatchAtStart(query);
        }

        [TestMethod]
        public void DateBraceMatcher_MatchesAtEndOfSecondHash()
        {
            var query = @"
                SELECT  #10/10/1993#|
                FROM    Employees
            ";

            AssertIsMatchAtEnd(query);
        }

        [TestMethod]
        public void DateBraceMatcher_DoesNotMatchInMiddle()
        {
            var query = @"
                SELECT  #10/|10/1993#
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void DateBraceMatcher_DoesNotMatchAtEndOfFirstHash()
        {
            var query = @"
                SELECT  #|10/10/1993#
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void DateBraceMatcher_DoesNotMatchAtStartOfSecondHash()
        {
            var query = @"
                SELECT  #10/10/1993|#
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }
    }
}