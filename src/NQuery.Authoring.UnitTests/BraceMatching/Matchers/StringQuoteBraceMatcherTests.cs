using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class StringQuoteBraceMatcherTests : BraceMatcherTests
    {
        public StringQuoteBraceMatcherTests()
            : base("'")
        {
        }

        protected override IBraceMatcher CreateMatcher()
        {
            return new StringQuoteBraceMatcher();
        }

        [TestMethod]
        public void StringQuoteBraceMatcher_MatchesAtStartOfFirstQuote()
        {
            var query = @"
                SELECT  |'An employee'
                FROM    Employees
            ";

            AssertIsMatchAtStart(query);
        }

        [TestMethod]
        public void StringQuoteBraceMatcher_MatchesAtEndOfSecondQuote()
        {
            var query = @"
                SELECT  'An employee'|
                FROM    Employees
            ";

            AssertIsMatchAtEnd(query);
        }

        [TestMethod]
        public void StringQuoteBraceMatcher_DoesNotMatchInMiddle()
        {
            var query = @"
                SELECT  'An |employee'
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void StringQuoteBraceMatcher_DoesNotMatchAtEndOfFirstQuote()
        {
            var query = @"
                SELECT  '|An employee'
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void StringQuoteBraceMatcher_DoesNotMatchAtStartOfSecondQuote()
        {
            var query = @"
                SELECT  'An employee|'
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }
    }
}