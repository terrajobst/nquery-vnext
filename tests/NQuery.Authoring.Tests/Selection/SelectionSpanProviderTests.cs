using NQuery.Authoring.Selection;

namespace NQuery.Authoring.Tests.Selection;

public abstract class SelectionSpanProviderTests
{
    protected abstract ISelectionSpanProvider CreateProvider();

    protected void AssertIsMatch(string queryWithMarkers)
    {
        var query = queryWithMarkers.ParseSpans(out var spans);

        var services = DocumentFactory.ServicesWithOnly(CreateProvider());
        var document = DocumentFactory.CreateQuery(query, services);
        var selection = document.Services.GetService<SelectionService>();

        var childParent = spans.Zip(spans.Skip(1), (c, p) => new { Child = c, Parent = p });

        foreach (var cp in childParent)
        {
            var child = cp.Child;
            var parent = cp.Parent;

            var view = DocumentView.Create(document, child.Start, child);
            var actual = selection.ExtendSelection(view);
            Assert.Equal(parent, actual);
        }
    }
}
