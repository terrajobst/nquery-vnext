using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

using Xunit;

namespace NQuery.Authoring.Tests.BraceMatching.Matchers
{
    public class CaseBraceMatcherTests : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new CaseBraceMatcher();
        }

        [Fact]
        public void CaseBraceMatcher_Matches()
        {
            var query = @"
                SELECT  {CASE}
                            WHEN ReportsTo IS NULL THEN
                                'The Boss'
                            ELSE
                                'Minion'
                        {END}
                FROM    Employees
            ";

            AssertIsMatch(query);
        }
    }
}