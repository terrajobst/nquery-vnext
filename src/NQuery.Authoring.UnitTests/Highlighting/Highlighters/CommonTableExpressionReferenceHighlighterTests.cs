using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.UnitTests.Highlighting.Highlighters
{
    [TestClass]
    public class CommonTableExpressionReferenceHighlighterTests : HighlighterTests
    {
        protected override IHighlighter CreateHighlighter()
        {
            return new SymbolReferenceHighlighter();
        }

        [TestMethod]
        public void CommonTableExpressionReferenceHighlighter_Matches()
        {
            var query = @"
                WITH {Emps} (
                    SELECT  *
                    FROM    Employees
                )
                SELECT  *
                FROM    {Emps} em
            ";

            AssertIsMatch(query);
        }
    }
}