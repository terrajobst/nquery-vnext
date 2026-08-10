using NQuery.Authoring.Outlining;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests.Outlining;

public abstract class OutlinerTests
{
    protected abstract IOutliner CreateOutliner();

    protected void AssertIsNoMatch(string query)
    {
        var document = CreateDocument(query);

        var actualRegions = document.Services.GetService<OutliningService>().FindRegions(document);

        Assert.Empty(actualRegions);
    }

    protected void AssertIsMatch(string queryWithMarkers, string expectedText)
    {
        var query = queryWithMarkers.ParseSingleSpan(out var expectedSpan);

        var document = CreateDocument(query);
        var documentSpan = document.GetSyntaxTree().Root.FullSpan;

        AssertMatches(document, documentSpan, expectedSpan, expectedText);
        AssertMatches(document, expectedSpan, expectedSpan, expectedText);
    }

    private Document CreateDocument(string query)
    {
        var services = DocumentFactory.ServicesWithOnly(CreateOutliner());
        return DocumentFactory.CreateQuery(query, services);
    }

    private static void AssertMatches(Document document, TextSpan span, TextSpan expectedSpan, string expectedText)
    {
        var actualRegions = document.Services.GetService<OutliningService>().FindRegions(document, span);

        var actualRegion = Assert.Single(actualRegions);
        Assert.Equal(expectedSpan, actualRegion.Span);
        Assert.Equal(expectedText, actualRegion.Text);
    }
}
