using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Xunit;

using NQuery.Authoring.Highlighting;
using NQuery.Text;

namespace NQuery.Authoring.UnitTests.Highlighting
{
    public abstract class HighlighterTests
    {
        protected abstract IHighlighter CreateHighlighter();

        protected void AssertIsMatch(string queryWithMarkers)
        {
            ImmutableArray<TextSpan> expectedSpans;
            var query = queryWithMarkers.ParseSpans(out expectedSpans);

            var compilation = CompilationFactory.CreateQuery(query);
            var semanticModel = compilation.GetSemanticModel();

            var highlighter = CreateHighlighter();
            var highlighters = ImmutableArray.Create(highlighter);

            Assert.True(expectedSpans.Length > 0);

            foreach (var span in expectedSpans)
                AssertIsMatch(semanticModel, span, highlighters, expectedSpans);
        }

        private static void AssertIsMatch(SemanticModel semanticModel, TextSpan span, ImmutableArray<IHighlighter> highlighters, ImmutableArray<TextSpan> expectedSpans)
        {
            var start = span.Start;
            var middle = span.Start + span.Length/2;
            var end = span.End;

            AssertMatches(semanticModel, start, highlighters, expectedSpans);
            AssertMatches(semanticModel, middle, highlighters, expectedSpans);
            AssertMatches(semanticModel, end, highlighters, expectedSpans);
        }

        private static void AssertMatches(SemanticModel semanticModel, int position, IEnumerable<IHighlighter> highlighters, ImmutableArray<TextSpan> expectedMatches)
        {
            var actualHighlights = semanticModel.GetHighlights(position, highlighters).ToImmutableArray();
            Assert.Equal(expectedMatches.AsEnumerable(), actualHighlights.AsEnumerable());
        }
    }
}