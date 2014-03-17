using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class QuotedIdentifierBraceMatcherTests : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new IdentifierBraceMatcher();
        }

        [TestMethod]
        public void IdentifierBraceMatcher_MatchesQuotes()
        {
            var query = @"
                SELECT  {""}FirstName{""}
                FROM    Employees
            ";

            AssertIsMatch(query);
        }
    }
}