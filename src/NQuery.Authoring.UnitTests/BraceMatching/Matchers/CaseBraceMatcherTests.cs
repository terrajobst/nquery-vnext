using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class CaseBraceMatcherTests : BraceMatcherTests
    {
        protected override IBraceMatcher CreateMatcher()
        {
            return new CaseBraceMatcher();
        }

        [TestMethod]
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