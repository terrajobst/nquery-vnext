using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class DateBraceMatcherTests : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new DateBraceMatcher();
        }

        [TestMethod]
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