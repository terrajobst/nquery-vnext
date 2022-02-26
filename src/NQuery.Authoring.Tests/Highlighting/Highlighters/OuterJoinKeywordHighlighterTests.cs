using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters
{
    public class OuterJoinKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new OuterJoinKeywordHighlighter();
        }

        [Fact]
        public void OuterJoinKeywordHighlighter_MatchesLeftOuter()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            {LEFT} {OUTER} {JOIN} EmployeeTerritories et {ON} et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [Fact]
        public void OuterJoinKeywordHighlighter_MatchesLeft()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            {LEFT} {JOIN} EmployeeTerritories et {ON} et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }
    }
}