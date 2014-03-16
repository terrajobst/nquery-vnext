using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Highlighting;

namespace NQuery.Authoring.UnitTests.Highlighting
{
    public abstract class HighlighterTests
    {
        protected void AssertIsMatch(string query)
        {
            int position;
            var compilation = CompilationFactory.CreateQuery(query, out position);
            var textBuffer = compilation.SyntaxTree.TextBuffer;
            var semanticModel = compilation.GetSemanticModel();

            var highligher = CreateHighligher();
            var highlighers = new[] { highligher };
            var actualHighlights = semanticModel.GetHighlights(position, highlighers).ToArray();

            var expectedHighlights = GetExpectedHighlights();

            Assert.AreEqual(expectedHighlights.Length, actualHighlights.Length);

            var lastEnd = 0;

            for (var i = 0; i < actualHighlights.Length; i++)
            {
                var expectedText = expectedHighlights[i];
                var actualHighlight = actualHighlights[i];
                var actualText = textBuffer.GetText(actualHighlight);

                Assert.IsTrue(actualHighlight.Start >= lastEnd);
                Assert.AreEqual(expectedText, actualText);

                lastEnd = actualHighlight.End;
            }
        }

        protected abstract IHighlighter CreateHighligher();

        protected abstract string[] GetExpectedHighlights();
    }
}