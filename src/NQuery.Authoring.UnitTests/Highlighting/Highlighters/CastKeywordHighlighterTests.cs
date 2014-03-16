using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class CastKeywordHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighligher()
        {
            return new CastKeywordHighlighter();
        }

        protected override string[] GetExpectedHighlights()
        {
            return new[] {"CAST", "AS"};
        }

        // CAST

        [TestMethod]
        public void CastKeywordHighlighter_MatchesAtStartOfCast()
        {
            var query = @"
                SELECT |CAST(1 AS FLOAT)
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CastKeywordHighlighter_MatchesInMiddleOfCast()
        {
            var query = @"
                SELECT CA|ST(1 AS FLOAT)
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CastKeywordHighlighter_MatchesAtEndOfCast()
        {
            var query = @"
                SELECT CAST|(1 AS FLOAT)
            ";

            AssertIsMatch(query);
        }

        // AS

        [TestMethod]
        public void CastKeywordHighlighter_MatchesAtStartOfAs()
        {
            var query = @"
                SELECT CAST(1 |AS FLOAT)
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CastKeywordHighlighter_MatchesInMiddleOfAs()
        {
            var query = @"
                SELECT CAST(1 A|S FLOAT)
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CastKeywordHighlighter_MatchesAtEndOfAs()
        {
            var query = @"
                SELECT CAST(1 AS| FLOAT)
            ";

            AssertIsMatch(query);
        }
    }
}