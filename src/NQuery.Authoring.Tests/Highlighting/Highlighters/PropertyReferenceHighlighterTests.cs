using System;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

using Xunit;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters
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