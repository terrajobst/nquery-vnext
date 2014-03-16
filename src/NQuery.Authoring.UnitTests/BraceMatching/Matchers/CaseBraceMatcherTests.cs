using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.UnitTests.BraceMatching.Matchers
{
    [TestClass]
    public class CaseBraceMatcherTests : BraceMatcherTests
    {
        public CaseBraceMatcherTests()
            : base("CASE", "END")
        {
        }

        protected override IBraceMatcher CreateMatcher()
        {
            return new CaseBraceMatcher();
        }

        [TestMethod]
        public void CaseBraceMatcher_MatchesAtStartOfCase()
        {
            var query = @"
                SELECT  |CASE
                            WHEN ReportsTo IS NULL THEN
                                'The Boss'
                            ELSE
                                'Minion'
                        END
                FROM    Employees
            ";

            AssertIsMatchAtStart(query);
        }

        [TestMethod]
        public void CaseBraceMatcher_MatchesAtEndOfEnd()
        {
            var query = @"
                SELECT  CASE
                            WHEN ReportsTo IS NULL THEN
                                'The Boss'
                            ELSE
                                'Minion'
                        END|
                FROM    Employees
            ";

            AssertIsMatchAtEnd(query);
        }

        [TestMethod]
        public void CaseBraceMatcher_DoesNotMatchInMiddle()
        {
            var query = @"
                SELECT  CASE
                            |WHEN ReportsTo IS NULL THEN
                                'The Boss'
                            ELSE
                                'Minion'
                        END
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void CaseBraceMatcher_DoesNotMatchAtEndOfCase()
        {
            var query = @"
                SELECT  CASE|
                            WHEN ReportsTo IS NULL THEN
                                'The Boss'
                            ELSE
                                'Minion'
                        END
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }

        [TestMethod]
        public void CaseBraceMatcher_DoesNotMatchAtStartOfEnd()
        {
            var query = @"
                SELECT  CASE
                            WHEN ReportsTo IS NULL THEN
                                'The Boss'
                            ELSE
                                'Minion'
                        |END
                FROM    Employees
            ";

            AssertIsNoMatch(query);
        }
    }
}