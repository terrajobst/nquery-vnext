using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class CommonTableExpressionReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighligher()
        {
            return new SymbolReferenceHighlighter();
        }

        protected override string[] GetExpectedHighlights()
        {
            return new[] { "Emps", "Emps", };
        }

        // Usage

        [TestMethod]
        public void CommonTableExpressionReferenceHighlighter_MatchesAtStartOfUsage()
        {
            var query = @"
                WITH Emps (
                    SELECT  *
                    FROM    Employees
                )
                SELECT  *
                FROM    |Emps em
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CommonTableExpressionReferenceHighlighter_MatchesInMiddleOfUsage()
        {
            var query = @"
                WITH Emps (
                    SELECT  *
                    FROM    Employees
                )
                SELECT  *
                FROM    Em|ps em
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CommonTableExpressionReferenceHighlighter_MatchesAtEndOfUsage()
        {
            var query = @"
                WITH Emps (
                    SELECT  *
                    FROM    Employees
                )
                SELECT  *
                FROM    Emps| em
            ";

            AssertIsMatch(query);
        }

        // Definition

        [TestMethod]
        public void CommonTableExpressionReferenceHighlighter_MatchesAtStartOfDefinition()
        {
            var query = @"
                WITH |Emps (
                    SELECT  *
                    FROM    Employees
                )
                SELECT  *
                FROM    Emps em
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CommonTableExpressionReferenceHighlighter_MatchesInMiddleOfDefinition()
        {
            var query = @"
                WITH Em|ps (
                    SELECT  *
                    FROM    Employees
                )
                SELECT  *
                FROM    Emps em
            ";

            AssertIsMatch(query);
        }

        [TestMethod]
        public void CommonTableExpressionReferenceHighlighter_MatchesAtEndOfDefinition()
        {
            var query = @"
                WITH Emps| (
                    SELECT  *
                    FROM    Employees
                )
                SELECT  *
                FROM    Emps em
            ";

            AssertIsMatch(query);
        }
    }
}