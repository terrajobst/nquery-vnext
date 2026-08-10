using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;

namespace NQuery.Authoring.Tests.Completion.Providers;

public class CommonTableExpressionCompletionProviderTests
{
    private static void AssertIsMatch(string queryWithPosition)
    {
        var completionModel = GetCompletionModel(queryWithPosition);
        var item = completionModel.Items.Single();

        Assert.Null(item.Glyph);
        Assert.Null(item.Description);
        Assert.Equal("rec", item.DisplayText);
        Assert.Equal("rec", item.InsertionText);
        Assert.Null(item.Symbol);
        Assert.True(item.IsBuilder);
    }

    private static void AssertIsNoMatch(string queryWithPosition)
    {
        var completionModel = GetCompletionModel(queryWithPosition);

        Assert.Empty(completionModel.Items);
    }

    private static CompletionModel GetCompletionModel(string queryWithPosition)
    {
        var query = queryWithPosition.ParseSinglePosition(out var position);

        var services = DocumentFactory.ServicesWithOnly<ICompletionProvider>(new CommonTableExpressionCompletionProvider());
        var document = DocumentFactory.CreateQuery(query, services);

        return document.Services.GetService<CompletionService>().GetModel(DocumentView.Create(document, position));
    }

    [Fact]
    public void CommonTableExpressionCompletionProvider_ReturnsBuilder_WhenValidPrefixIsPresent()
    {
        var query = """
            WITH rec|
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void CommonTableExpressionCompletionProvider_ReturnsNoBuilder_WhenRecursiveIsMissing()
    {
        var query = """
            WITH |
            """;

        AssertIsNoMatch(query);
    }

    [Fact]
    public void CommonTableExpressionCompletionProvider_ReturnsNoBuilder_WhenRecursiveIsPresent()
    {
        var query = """
            WITH recursive|
            """;

        AssertIsNoMatch(query);
    }

    [Fact]
    public void CommonTableExpressionCompletionProvider_ReturnsNoBuilder_WhenInQuery()
    {
        var query = """
            WITH Emps AS
            (
                SELECT  |
                FROM    Employees e
            )
            SELECT  *
            FROM    Emps
            """;

        AssertIsNoMatch(query);
    }
}
