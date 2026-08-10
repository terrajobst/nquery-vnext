using NQuery.Authoring.QuickInfo;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests.QuickInfo;

public abstract class QuickInfoModelProviderTests
{
    protected abstract IQuickInfoModelProvider CreateProvider();

    protected abstract QuickInfoModel CreateExpectedModel(SemanticModel semanticModel);

    protected void AssertIsMatch(string query)
    {
        AssertIsMatch(query, null);
    }

    protected void AssertIsMatch(string query, Func<Catalog, Catalog>? catalogModifer)
    {
        GetModels(query, catalogModifer, out var semanticModel, out var startModel, out var middleModel, out var endModel);

        var expectedModel = CreateExpectedModel(semanticModel);

        AssertIsMatch(expectedModel, startModel);
        AssertIsMatch(expectedModel, middleModel);
        AssertIsMatch(expectedModel, endModel);
    }

    protected void AssertIsNotMatch(string query)
    {
        AssertIsNotMatch(query, null);
    }

    protected void AssertIsNotMatch(string query, Func<Catalog, Catalog>? catalogModifer)
    {
        GetModels(query, catalogModifer, out _, out var startModel, out var middleModel, out var endModel);

        Assert.Null(startModel);
        Assert.Null(middleModel);
        Assert.Null(endModel);
    }

    private static void AssertIsMatch(QuickInfoModel expectedModel, QuickInfoModel? actualModel)
    {
        Assert.NotNull(actualModel);

        Assert.Equal(expectedModel.SemanticModel, actualModel.SemanticModel);
        Assert.Equal(expectedModel.Span, actualModel.Span);
        Assert.Equal(expectedModel.Glyph, actualModel.Glyph);
        Assert.Equal(expectedModel.Markup, actualModel.Markup);
    }

    private void GetModels(string query, Func<Catalog, Catalog>? catalogModifer, out SemanticModel semanticModel, out QuickInfoModel? startModel, out QuickInfoModel? middleModel, out QuickInfoModel? endModel)
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

        startModel = quickInfo.GetModel(DocumentView.Create(document, start));
        middleModel = quickInfo.GetModel(DocumentView.Create(document, middle));
        endModel = quickInfo.GetModel(DocumentView.Create(document, end));
    }
}
