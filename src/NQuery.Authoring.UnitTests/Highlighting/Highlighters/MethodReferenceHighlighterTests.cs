using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class MethodReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [TestMethod]
        public void MethodReferenceHighlighter_Matches()
        {
            var query = @"
                SELECT  e.FirstName.{Substring}(1),
                        e.LastName.{Substring}(2),
                        e.LastName.Substring(3, 1)
                FROM    Employees e
            ";

            AssertIsMatch(query);
        }
    }
}