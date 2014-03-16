using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class AggregateReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighligher()
        {
            return new SymbolReferenceHighlighter();
        }

        protected override string[] GetExpectedHighlights()
        {
            return new[] {"COUNT", "COUNT"};
        }

        [TestMethod]
        public void AggregateReferenceHighlighter_MatchesAtStart()
        {
            var query = @"
                SELECT  |COUNT(*),
                        COUNT(e.ReportsTo)
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void AggregateReferenceHighlighter_MatchesInMiddle()
        {
            var query = @"
                SELECT  CO|UNT(*),
                        COUNT(e.ReportsTo)
                FROM    Employees e
                WHERE   e.FirstName = 'Andrew'
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void AggregateReferenceHighlighter_MatchesAtEnd()
        {
            var query = @"
                SELECT  COUNT|(*),
                        COUNT(e.ReportsTo)
                FROM    Employees e
                WHERE   e.FirstName = 'Andrew'
            ";

            AssertIsMatch(query);
        }
    }
}