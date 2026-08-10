using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Tests.Completion.Providers;

public class TypeCompletionProviderTests
{
    private static CompletionModel GetCompletionModel(string queryWithJoinMarker)
    {
        var services = DocumentFactory.ServicesWithOnly<ICompletionProvider>(new TypeCompletionProvider());
        var document = DocumentFactory.CreateQuery(queryWithJoinMarker, out int position, services);

        return document.Services.GetService<CompletionService>().GetModel(DocumentView.Create(document, position));
    }

    private static void AssertIsMatch(string query)
    {
        var completionModel = GetCompletionModel(query);
        var items = completionModel.Items.ToDictionary(i => i.InsertionText);
        var typeNames = SyntaxFacts.GetTypeNames();

        foreach (var typeName in typeNames)
        {
            var item = items[typeName];
            Assert.Equal(Glyph.Type, item.Glyph);
            Assert.Equal(typeName, item.Description);
            Assert.Equal(typeName, item.DisplayText);
            Assert.Equal(typeName, item.InsertionText);
            Assert.Null(item.Symbol);
        }
    }

    private static void AssertIsNotMatch(string query)
    {
        var completionModel = GetCompletionModel(query);
        var items = completionModel.Items.ToDictionary(i => i.InsertionText);
        var typeNames = SyntaxFacts.GetTypeNames();

        var returnsAnyTypes = typeNames.Any(items.ContainsKey);
        Assert.False(returnsAnyTypes);
    }

    [Fact]
    public void TypeCompletionProvider_ReturnsTypes()
    {
        var query = """
            SELECT  CAST(1 AS |
            """;

        AssertIsMatch(query);
    }

    [Fact]
    public void TypeCompletionProvider_DoesNotReturnTypes_IfNoAs()
    {
        var query = """
            SELECT  CAST(1 |
            """;

        AssertIsNotMatch(query);
    }
}
