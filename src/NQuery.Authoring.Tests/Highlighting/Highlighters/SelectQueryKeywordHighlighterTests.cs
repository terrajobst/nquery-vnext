using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters
{
    public class SelectQueryKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SelectQueryKeywordHighlighter();
        }

        [Fact]
        public void SelectQueryKeywordHighlighter_Matches()
        {
            var query = @"
                {SELECT} e.City,
                         COUNT(*) [#Employees]
                {FROM}   Employees e
                {WHERE}  e.ReportsTo IS NOT NULL
                {GROUP   BY} e.City
                {HAVING} COUNT(*) > 1
            ";

            AssertIsMatch(query);
        }
    }
}