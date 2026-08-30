using NQuery.Authoring.QuickInfo;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests.QuickInfo;

public abstract class QuickInfoProviderTests
{
    protected abstract IQuickInfoProvider CreateProvider();

    protected abstract QuickInfoResult CreateExpectedResult(SemanticModel semanticModel);

    protected void AssertIsMatch(string query)
    {
        AssertIsMatch(query, null);
    }

    protected void AssertIsMatch(string query, Func<Catalog, Catalog>? catalogModifer)
    {
        GetResults(query, catalogModifer, out var semanticModel, out var startResult, out var middleResult, out var endResult);

        var expectedResult = CreateExpectedResult(semanticModel);

        AssertIsMatch(expectedResult, startResult);
        AssertIsMatch(expectedResult, middleResult);
        AssertIsMatch(expectedResult, endResult);
    }

    protected void AssertIsNotMatch(string query)
    {
        AssertIsNotMatch(query, null);
    }

    protected void AssertIsNotMatch(string query, Func<Catalog, Catalog>? catalogModifer)
    {
        GetResults(query, catalogModifer, out _, out var startResult, out var middleResult, out var endResult);

        Assert.Null(startResult);
        Assert.Null(middleResult);
        Assert.Null(endResult);
    }

    private static void AssertIsMatch(QuickInfoResult expectedResult, QuickInfoResult? actualResult)
    {
        Assert.NotNull(actualResult);

        Assert.Equal(expectedResult.SemanticModel, actualResult.SemanticModel);
        Assert.Equal(expectedResult.Span, actualResult.Span);
        Assert.Equal(expectedResult.Glyph, actualResult.Glyph);
        Assert.Equal(expectedResult.Markup, actualResult.Markup);
    }

    private void GetResults(string query, Func<Catalog, Catalog>? catalogModifer, out SemanticModel semanticModel, out QuickInfoResult? startResult, out QuickInfoResult? middleResult, out QuickInfoResult? endResult)
    {
        var services = DocumentFactory.ServicesWithOnly(CreateProvider());
        var document = DocumentFactory.CreateQuery(query, out TextSpan span, services);

        if (catalogModifer is not null)
            document = document.WithCatalog(catalogModifer(document.Catalog));

        semanticModel = document.GetSemanticModel();
        var start = span.Start;
        var middle = span.Start + span.Length / 2;
        var end = span.End;

        var quickInfo = document.Services.GetService<QuickInfoService>();

        startResult = quickInfo.GetResult(DocumentView.Create(document, start));
        middleResult = quickInfo.GetResult(DocumentView.Create(document, middle));
        endResult = quickInfo.GetResult(DocumentView.Create(document, end));
    }
}
