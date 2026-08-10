using NQuery.Authoring.Commenting;

namespace NQuery.Authoring.Tests.Commenting;

public abstract class CommenterTests
{
    protected abstract Document ToggleComment(CommentingService service, DocumentView view);

    protected void AssertIsMatch(string queryWithMarkers, string expectedQuery)
    {
        var query = queryWithMarkers.ParseSingleSpan(out var selection);

        var document = DocumentFactory.CreateQuery(query);
        var view = DocumentView.Create(document, selection.Start, selection);

        var actual = ToggleComment(document.Services.GetService<CommentingService>(), view);
        var actualQuery = actual.Text.GetText();

        Assert.Equal(expectedQuery, actualQuery);
    }
}
