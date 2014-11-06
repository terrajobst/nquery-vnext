using System;

using Xunit;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    public class PropertyReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [Fact]
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