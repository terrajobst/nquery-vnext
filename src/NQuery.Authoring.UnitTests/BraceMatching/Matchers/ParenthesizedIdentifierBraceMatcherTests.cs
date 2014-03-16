using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class ParenthesizedIdentifierBraceMatcherTests : BraceMatcherTests
    {
        public ParenthesizedIdentifierBraceMatcherTests()
            : base("[", "]")
        {
        }

        protected override IBraceMatcher CreateMatcher()
        {
            return new IdentifierBraceMatcher();
        }

        [TestMethod]
        public void IdentifierBraceMatcher_MatchesAtEndOfRightBracket()
        {
            var query = @"
                SELECT  [FirstName]|
                FROM    Employees
            ";

            AssertIsMatchAtEnd(query);
        }

        [TestMethod]
        public void IdentifierBraceMatcher_DoesNotMatchInMiddleOfParenthesizedIdentifer()
        {
            var query = @"
                SELECT  [First|Name]
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void IdentifierBraceMatcher_DoesNotMatchAtEndOfLeftBracket()
        {
            var query = @"
                SELECT  [|FirstName]
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void IdentifierBraceMatcher_DoesNotMatchAtStartOfRightBracket()
        {
            var query = @"
                SELECT  [FirstName|]
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }
    }
}