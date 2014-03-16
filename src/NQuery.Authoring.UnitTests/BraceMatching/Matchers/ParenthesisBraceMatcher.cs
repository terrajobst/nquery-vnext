using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class ParenthesisBraceMatcherTest : BraceMatcherTests
    {
        public ParenthesisBraceMatcherTest()
            : base("(", ")")
        {
        }

        protected override IBraceMatcher CreateMatcher()
        {
            return new ParenthesisBraceMatcher();
        }

        [TestMethod]
        public void ParenthesisBraceMatcher_MatchesAtStartOfFirstParenthesis()
        {
            var query = @"
                SELECT  |(FirstName + ' ' + LastName)
                FROM    Employees
            ";

            AssertIsMatchAtStart(query);
        }

        [TestMethod]
        public void ParenthesisBraceMatcher_MatchesAtEndOfSecondParenthesis()
        {
            var query = @"
                SELECT  (FirstName + ' ' + LastName)|
                FROM    Employees
            ";

            AssertIsMatchAtEnd(query);
        }

        [TestMethod]
        public void ParenthesisBraceMatcher_DoesNotMatchInMiddleO()
        {
            var query = @"
                SELECT  (FirstName| + ' ' + LastName)
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void ParenthesisBraceMatcher_DoesNotMatchAtEndOfFirstParenthesis()
        {
            var query = @"
                SELECT  (|FirstName + ' ' + LastName)
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void ParenthesisBraceMatcher_DoesNotMatchAtStartOfSecondParenthesis()
        {
            var query = @"
                SELECT  (FirstName + ' ' + LastName|)
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }
    }
}