using System;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

using Xunit;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters
{
    public class OrderedQueryKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new OrderedQueryKeywordHighlighter();
        }

        [Fact]
        public void OrderedQueryKeywordHighlighter_Matches()
        {
            var query = @"
                {SELECT} e.City,
                         COUNT(*) [#Employees]
                {FROM}   Employees e
                {WHERE}  e.ReportsTo IS NOT NULL
                {GROUP   BY} e.City
                {HAVING} COUNT(*) > 1
                {ORDER   BY} [#Employees] DESC
            ";

            AssertIsMatch(query);
        }
    }
}