using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class PropertyReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [TestMethod]
        public void PropertyReferenceHighlighter_Matches()
        {
            var query = @"
                SELECT  e.FirstName.{Length},
                        e.LastName.{Length},
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }
    }
}