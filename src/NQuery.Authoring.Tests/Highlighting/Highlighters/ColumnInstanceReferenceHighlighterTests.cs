using System;

using Xunit;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    public class ColumnInstanceReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [Fact]
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