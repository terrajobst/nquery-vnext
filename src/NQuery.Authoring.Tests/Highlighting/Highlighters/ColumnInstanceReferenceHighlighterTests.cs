using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

using Xunit;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters
{
    public class ColumnInstanceReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [Fact]
        public void ColumnInstanceReferenceHighlighter_Matches()
        {
            var query = @"
                SELECT  e.{FirstName},
                        e.LastName
                FROM    Employees e
                WHERE   e.{FirstName} = 'Andrew'
            ";

            AssertIsMatch(query);
        }
    }
}