using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class CastKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new CastKeywordHighlighter();
        }

        [TestMethod]
        public void CastKeywordHighlighter_Matches()
        {
            var query = @"
                SELECT {CAST}(1 {AS} FLOAT)
            ";

            AssertIsMatch(query);
        }
    }
}