using System;

using Xunit;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    public class SelectColumnReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [Fact]
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