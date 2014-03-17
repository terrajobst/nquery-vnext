using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class InnerJoinKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new InnerJoinKeywordHighlighter();
        }

        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesInnerJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            {INNER} {JOIN} EmployeeTerritories et {ON} et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void InnerJoinKeywordHighlighter_MatchesJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            {JOIN} EmployeeTerritories et {ON} et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }
    }
}