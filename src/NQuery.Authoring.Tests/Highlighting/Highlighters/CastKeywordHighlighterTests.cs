using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters
{
    public class CastKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new CastKeywordHighlighter();
        }

        [Fact]
        public void CastKeywordHighlighter_Matches()
        {
            var query = @"
                SELECT {CAST}(1 {AS} FLOAT)
            ";

            AssertIsMatch(query);
        }
    }
}