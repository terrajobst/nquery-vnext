using System;

using Xunit;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    public class DerivedTableInstanceReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [Fact]
        public void DerivedTableInstanceReferenceHighlighter_Matches()
        {
            var query = @"
                SELECT  {em}.FirstName,
                        {em}.LastName
                FROM    (SELECT * FROM Employees) {em}
            ";

            AssertIsMatch(query);
        }
    }
}