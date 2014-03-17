using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class SelectColumnReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [TestMethod]
        public void SelectColumnReferenceHighlighter_Matches()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName {FullName}
                FROM    Employees e
                ORDER   BY {FullName}
            ";

            AssertIsMatch(query);
        }
    }
}