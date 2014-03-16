using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class QuotedIdentifierBraceMatcherTests : BraceMatcherTests
    {
        public QuotedIdentifierBraceMatcherTests()
            : base("\"")
        {
        }

        protected override IBraceMatcher CreateMatcher()
        {
            return new IdentifierBraceMatcher();
        }

        [TestMethod]
        public void IdentifierBraceMatcher_MatchesAtStartOfFirstQuote()
        {
            var query = @"
                SELECT  |""FirstName""
                FROM    Employees
            ";

            AssertIsMatchAtStart(query);
        }

        [TestMethod]
        public void IdentifierBraceMatcher_MatchesAtEndOfSecondQuote()
        {
            var query = @"
                SELECT  ""FirstName""|
                FROM    Employees
            ";

            AssertIsMatchAtEnd(query);
        }

        [TestMethod]
        public void IdentifierBraceMatcher_DoesNotMatchInMiddleOfQuotedIdentifier()
        {
            var query = @"
                SELECT  ""First|Name""
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void IdentifierBraceMatcher_DoesNotMatchAtEndOfFirstQuote()
        {
            var query = @"
                SELECT  ""|FirstName""
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void IdentifierBraceMatcher_DoesNotMatchAtStartOfSecondQuote()
        {
            var query = @"
                SELECT  ""FirstName|""
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }
    }
}