using System.Collections.Immutable;

using NQuery.Authoring.Highlighting;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests.Highlighting;

public abstract class HighlighterTests
{
    protected abstract IHighlighter CreateHighlighter();

    protected void AssertIsMatch(string queryWithMarkers)
    {
        var query = queryWithMarkers.ParseSpans(out var expectedSpans);

        var services = DocumentFactory.ServicesWithOnly(CreateHighlighter());
        var document = DocumentFactory.CreateQuery(query, services);

        Assert.True(expectedSpans.Length > 0);

        foreach (var span in expectedSpans)
            AssertIsMatch(document, span, expectedSpans);
    }

    private static void AssertIsMatch(Document document, TextSpan span, ImmutableArray<TextSpan> expectedSpans)
    {
        var start = span.Start;
        var middle = span.Start + span.Length / 2;
        var end = span.End;

        AssertMatches(document, start, expectedSpans);
        AssertMatches(document, middle, expectedSpans);
        AssertMatches(document, end, expectedSpans);
    }

    private static void AssertMatches(Document document, int position, ImmutableArray<TextSpan> expectedMatches)
    {
        var view = DocumentView.Create(document, position);
        var actualHighlights = document.Services.GetService<HighlightingService>().GetHighlights(view);
        Assert.Equal(expectedMatches.AsEnumerable(), actualHighlights.AsEnumerable());
    }
}
