using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.Tests.Highlighting.Highlighters
{
    public class MethodReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [Fact]
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