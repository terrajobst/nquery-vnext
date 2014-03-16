using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class OrderedQueryKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighligher()
        {
            return new OrderedQueryKeywordHighlighter();
        }

        protected override string[] GetExpectedHighlights()
        {
            return new[] { "SELECT", "FROM", "WHERE", "GROUP   BY", "HAVING", "ORDER   BY" };
        }

        // SELECT

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtBeginningOfSelect()
        {
            var query = @"
                |SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesInMiddleOfSelect()
        {
            var query = @"
                SEL|ECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtEndOfSelect()
        {
            var query = @"
                SELECT|  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        // FROM

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtBeginningOfFrom()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                |FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesInMiddleOfFrom()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FR|OM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtEndOfFrom()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM|    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        // WHERE

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtBeginningOfWhere()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                |WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesInMiddleOfWhere()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WH|ERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtEndOfWhere()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE|   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        // GROUP BY

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtBeginningOfGroup()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                |GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesInMiddleOfGroup()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GR|OUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtEndOfGroup()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP|   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtBeginningOfGroupBy()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   |BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesInMiddleOfGroupBy()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   B|Y e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtEndOfGroupBy()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY| e.City
                HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        // HAVING

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtBeginningOfHaving()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                |HAVING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesInMiddleOfHaving()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HA|VING  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtEndOfHaving()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING|  COUNT(*) > 1
                ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        // ORDER BY

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtBeginningOfOrder()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                |ORDER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesInMiddleOfOrder()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORD|ER   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtEndOfOrder()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER|   BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtBeginningOfOrderBy()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   |BY [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesInMiddleOfOrderBy()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   B|Y [#Employees] DESC
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OrderedQueryKeywordHighlighter_MatchesAtEndOfOrderBy()
        {
            var query = @"
                SELECT  e.City,
                        COUNT(*) [#Employees]
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
                GROUP   BY e.City
                HAVING  COUNT(*) > 1
                ORDER   BY| [#Employees] DESC
            ";

            AssertIsMatch(query);
        }
    }
}