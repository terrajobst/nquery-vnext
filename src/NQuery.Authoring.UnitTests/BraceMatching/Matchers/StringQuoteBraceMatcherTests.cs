using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class StringQuoteBraceMatcherTests : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new StringQuoteBraceMatcher();
        }

        [TestMethod]
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