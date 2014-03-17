using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class SelectQueryKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SelectQueryKeywordHighlighter();
        }

        [TestMethod]
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