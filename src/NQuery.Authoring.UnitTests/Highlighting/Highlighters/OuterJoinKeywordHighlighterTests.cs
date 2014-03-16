using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class OuterJoinKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighligher()
        {
            return new OuterJoinKeywordHighlighter();
        }

        protected override string[] GetExpectedHighlights()
        {
            return new[] {"LEFT", "OUTER", "JOIN", "ON"};
        }

        // LEFT

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesAtBeginningOfLeft()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            |LEFT OUTER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesInMiddleOfLeft()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LE|FT OUTER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesAtEndOfLeft()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT| OUTER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        // OUTER

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesAtBeginningOfOuter()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT |OUTER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesInMiddleOfOuter()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT OU|TER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesAtEndOfOuter()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT OUTER| JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }
        
        // JOIN

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesAtBeginningOfJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT OUTER |JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesInMiddleOfJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT OUTER JO|IN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesAtEndOfJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT OUTER JOIN| EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        // ON

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesAtBeginningOfOn()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT OUTER JOIN EmployeeTerritories et |ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesInMiddleOfOn()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT OUTER JOIN EmployeeTerritories et O|N et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void OuterJoinKeywordHighlighter_MatchesAtEndOfOn()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            LEFT OUTER JOIN EmployeeTerritories et ON| et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }
    }
}