using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class ColumnInstanceReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [TestMethod]
        public void ColumnInstanceReferenceHighlighter_Matches()
        {
            var query = @"
                SELECT  e.{FirstName},
                        e.LastName
                FROM    Employees e
                WHERE   e.{FirstName} = 'Andrew'
            ";

            AssertIsMatch(query);
        }
    }
}