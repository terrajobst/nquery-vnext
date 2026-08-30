using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;

namespace NQuery.Authoring.Tests.Completion.Providers;

public class AliasCompletionProviderTests
{
    private static void AssertIsMatch(string queryWithPosition)
    {
        var completionResult = GetCompletionResult(queryWithPosition);
        var item = completionResult.Items.Single();

        Assert.Null(item.Glyph);
        Assert.Null(item.Description);
        Assert.Equal("a", item.DisplayText);
        Assert.Equal("a", item.InsertionText);
        Assert.Null(item.Symbol);
        Assert.True(item.IsBuilder);
    }

    private static void AssertIsNoMatch(string queryWithPosition)
    {
        var completionResult = GetCompletionResult(queryWithPosition);

        Assert.Empty(completionResult.Items);
    }

    private static CompletionResult GetCompletionResult(string queryWithPosition)
    {
        var query = queryWithPosition.ParseSinglePosition(out var position);

        var services = DocumentFactory.ServicesWithOnly<ICompletionProvider>(new AliasCompletionProvider());
        var document = DocumentFactory.CreateQuery(query, services);

        return document.Services.GetService<CompletionService>().GetResult(DocumentView.Create(document, position));
    }

    [Fact]
    public void AliasCompletionProvider_ReturnsBuilder_WhenValidPrefixIsPresent()
    {
        var query = """
            SELECT  *
            FROM    Employees a|
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void AliasCompletionProvider_ReturnsNoBuilder_WhenAsIsMissing()
    {
        var query = """
            SELECT  *
            FROM    Employees |
            """;

        AssertIsNoMatch(query);
    }

    [Fact]
    public void AliasCompletionProvider_ReturnsNoBuilder_WhenAsIsPresent()
    {
        var query = """
            SELECT  *
            FROM    Employees as|
            """;

        AssertIsNoMatch(query);
    }
}
