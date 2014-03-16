using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class DerivedTableInstanceReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighligher()
        {
            return new SymbolReferenceHighlighter();
        }

        protected override string[] GetExpectedHighlights()
        {
            return new[] { "em", "em", "em" };
        }

        // Usage

        [TestMethod]
        public void DerivedTableInstanceReferenceHighlighter_MatchesAtStartOfUsage()
        {
            var query = @"
                SELECT  |em.FirstName,
                        em.LastName
                FROM    (SELECT * FROM Employees) em
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void DerivedTableInstanceReferenceHighlighter_MatchesInMiddleOfUsage()
        {
            var query = @"
                SELECT  e|m.FirstName,
                        em.LastName
                FROM    (SELECT * FROM Employees) em
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void DerivedTableInstanceReferenceHighlighter_MatchesAtEndOfUsage()
        {
            var query = @"
                SELECT  em|.FirstName,
                        em.LastName
                FROM    (SELECT * FROM Employees) em
            ";

            AssertIsMatch(query);
        }

        // Definition

        [TestMethod]
        public void DerivedTableInstanceReferenceHighlighter_MatchesAtStartOfDefinition()
        {
            var query = @"
                SELECT  em.FirstName,
                        em.LastName
                FROM    (SELECT * FROM Employees) |em
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void DerivedTableInstanceReferenceHighlighter_MatchesInMiddleOfDefinition()
        {
            var query = @"
                SELECT  em.FirstName,
                        em.LastName
                FROM    (SELECT * FROM Employees) e|m
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void DerivedTableInstanceReferenceHighlighter_MatchesAtEndOfDefinition()
        {
            var query = @"
                SELECT  em.FirstName,
                        em.LastName
                FROM    (SELECT * FROM Employees) em|
            ";

            AssertIsMatch(query);
        }
    }
}