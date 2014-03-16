using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class ColumnInstanceReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighligher()
        {
            return new SymbolReferenceHighlighter();
        }

        protected override string[] GetExpectedHighlights()
        {
            return new[] {"FirstName", "FirstName"};
        }

        [TestMethod]
        public void ColumnInstanceReferenceHighlighter_MatchesAtStart()
        {
            var query = @"
                SELECT  e.|FirstName,
                        e.LastName
                FROM    Employees e
                WHERE   e.FirstName = 'Andrew'
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void ColumnInstanceReferenceHighlighter_MatchesInMiddle()
        {
            var query = @"
                SELECT  e.First|Name,
                        e.LastName
                FROM    Employees e
                WHERE   e.FirstName = 'Andrew'
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void ColumnInstanceReferenceHighlighter_MatchesAtEnd()
        {
            var query = @"
                SELECT  e.FirstName|,
                        e.LastName
                FROM    Employees e
                WHERE   e.FirstName = 'Andrew'
            ";

            AssertIsMatch(query);
        }
    }
}