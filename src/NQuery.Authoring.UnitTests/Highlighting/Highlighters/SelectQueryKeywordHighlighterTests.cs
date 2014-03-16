using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class SelectQueryKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighligher()
        {
            return new SelectQueryKeywordHighlighter();
        }

        protected override string[] GetExpectedHighlights()
        {
            return new[] { "SELECT", "FROM", "WHERE", "GROUP   BY", "HAVING" };
        }


        // SELECT

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtBeginningOfSelect()
        {
            var query = @"
                |SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesInMiddleOfSelect()
        {
            var query = @"
                SEL|ECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtEndOfSelect()
        {
            var query = @"
                SELECT|  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        // FROM

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtBeginningOfFrom()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                |FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesInMiddleOfFrom()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FR|OM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtEndOfFrom()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM|    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        // WHERE

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtBeginningOfWhere()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                |WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesInMiddleOfWhere()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WH|ERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtEndOfWhere()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE|   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        // GROUP BY

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtBeginningOfGroup()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                |GROUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesInMiddleOfGroup()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GR|OUP   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtEndOfGroup()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP|   BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtBeginningOfGroupBy()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   |BY e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesInMiddleOfGroupBy()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   B|Y e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtEndOfGroupBy()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY| e.City
                HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        // HAVING

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtBeginningOfHaving()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                |HAVING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesInMiddleOfHaving()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HA|VING  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void SelectQueryKeywordHighlighter_MatchesAtEndOfHaving()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING|  COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }
    }
}