using System;

using Xunit;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    public class ParenthesizedIdentifierBraceMatcherTests : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new IdentifierBraceMatcher();
        }

        [Fact]
        public void IdentifierBraceMatcher_MatchesBrackets()
        {
            var query = @"
                SELECT  {[}FirstName{]}
                FROM    Employees
            ";

            AssertIsMatch(query);
        }
    }
}