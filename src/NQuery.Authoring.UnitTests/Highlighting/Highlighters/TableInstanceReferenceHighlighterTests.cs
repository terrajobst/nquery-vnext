using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class TableInstanceReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [TestMethod]
        public void TableInstanceReferenceHighlighter_Matches()
        {
            var query = @"
                SELECT  {em}.FirstName,
                        {em}.LastName
                FROM    Employees {em}
            ";

            AssertIsMatch(query);
        }
    }
}