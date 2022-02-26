using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

using Xunit;

namespace NQuery.Authoring.Tests.BraceMatching.Matchers
{
    public class IdentifierBraceMatcherTests : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new IdentifierBraceMatcher();
        }

        [Fact]
        public void IdentifierBraceMatcher_DoesNotMatchRegularIdentifier()
        {
            var query = @"
                SELECT  {}FirstName{}
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }
    }
}