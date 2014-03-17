using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class ParenthesisBraceMatcherTest : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new ParenthesisBraceMatcher();
        }

        [TestMethod]
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