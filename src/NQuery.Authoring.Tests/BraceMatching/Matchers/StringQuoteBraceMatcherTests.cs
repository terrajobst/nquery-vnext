using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.Tests.BraceMatching.Matchers
{
    public class StringQuoteBraceMatcherTests : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new StringQuoteBraceMatcher();
        }

        [Fact]
        public void StringQuoteBraceMatcher_Matches()
        {
            var query = @"
                SELECT  {'}An employee{'}
                FROM    Employees
            ";

            AssertIsMatch(query);
        }
    }
}