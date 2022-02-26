using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.Tests.BraceMatching.Matchers
{
    public class ParenthesisBraceMatcherTest : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new ParenthesisBraceMatcher();
        }

        [Fact]
        public void ParenthesisBraceMatcher_Matches()
        {
            var query = @"
                SELECT  {(}FirstName + ' ' + LastName{)}
                FROM    Employees
            ";

            AssertIsMatch(query);
        }
    }
}