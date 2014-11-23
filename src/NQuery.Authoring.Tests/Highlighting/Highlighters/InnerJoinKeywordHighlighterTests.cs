using System;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

using Xunit;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters
{
    public class InnerJoinKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new InnerJoinKeywordHighlighter();
        }

        [Fact]
        public void InnerJoinKeywordHighlighter_MatchesInnerJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                            {INNER} {JOIN} EmployeeTerritories et {ON} et.EmployeeID = e.EmployeeID
            ";

            AssertIsMatch(query);
        }

        [Fact]
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