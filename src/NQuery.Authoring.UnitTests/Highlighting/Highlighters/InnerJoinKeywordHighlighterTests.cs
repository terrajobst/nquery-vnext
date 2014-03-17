using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class InnerJoinKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighligher()
        {
            return new InnerJoinKeywordHighlighter();
        }

        protected override string[] GetExpectedHighlights()
        {
            return new[] {"INNER", "JOIN", "ON"};
        }

        // INNER

        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesAtBeginningOfInner()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            |INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesInMiddleOfInner()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            IN|NER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesAtEndOfInner()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER| JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        // JOIN
        
        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesAtBeginningOfJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER |JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesInMiddleOfJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER JO|IN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesAtEndOfJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER JOIN| EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        // ON

        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesAtBeginningOfOn()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER JOIN EmployeeTerritories et |ON et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesInMiddleOfOn()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER JOIN EmployeeTerritories et O|N et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesAtEndOfOn()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            INNER JOIN EmployeeTerritories et ON| et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }
    }
}