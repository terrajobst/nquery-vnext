using System;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

using Xunit;

namespace NQuery.Authoring.Tests.BraceMatching.Matchers
{
    public class DateBraceMatcherTests : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new DateBraceMatcher();
        }

        [Fact]
        public void DateBraceMatcher_Matches()
        {
            var query = @"
                SELECT  {#}10/10/1993{#}
                FROM    Employees
            ";

            AssertIsMatch(query);
        }
    }
}