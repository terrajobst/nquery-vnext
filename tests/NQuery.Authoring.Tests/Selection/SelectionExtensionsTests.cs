using NQuery.Authoring.Selection;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests.Selection;

public class SelectionExtensionsTests : ExtensionTests
{
    [Fact]
    public void SelectionExtensions_ReturnsAllProviders()
    {
        AssertAllProvidersAreExposed<ISelectionSpanProvider>();
    }

    [Fact]
    public void SelectionExtensions_Grows()
    {
        var query = """
            SELECT  e.First|Name
            FROM    Employees e
            """;

        var document = DocumentFactory.CreateQuery(query, out int position);
        var text = document.Text;
        var selection = document.Services.GetService<SelectionService>();

        TextSpan Extend(TextSpan span)
        {
            return selection.ExtendSelection(DocumentView.Create(document, position, span));
        }

        var start = new TextSpan(position, 0);

        var firstTime = Extend(start);
        Assert.Equal("FirstName", text.GetText(firstTime));

        var secondTime = Extend(firstTime);
        Assert.Equal("e.FirstName", text.GetText(secondTime));

        var thirdTime = Extend(secondTime);
        Assert.Equal("SELECT  e.FirstName", text.GetText(thirdTime));

        var fourthTime = Extend(thirdTime);
        Assert.Equal(text.GetText().Trim(), text.GetText(fourthTime));

        var fifthTime = Extend(fourthTime);
        Assert.Equal(text.GetText().TrimStart(), text.GetText(fifthTime));

        var sixthTime = Extend(fifthTime);
        Assert.Equal(fifthTime, sixthTime);
    }
}
