using Xunit;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    public class CaseKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new CaseKeywordHighlighter();
        }

        [Fact]
        public void CaseKeywordHighlighter_Matches()
        {
            var query = @"
                SELECT  {CASE}
                            {WHEN} e.EmployeeID = 1 {THEN} 'One'
                            {WHEN} e.EmployeeID = 2 {THEN} 'Then'
                            {ELSE} 'Other'
                        {END}
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }
    }
}