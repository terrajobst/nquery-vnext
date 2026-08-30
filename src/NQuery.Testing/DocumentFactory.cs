using System.Collections.Immutable;

using NQuery.Authoring;
using NQuery.CodeAnalysis.Text;
using NQuery.Northwind;

namespace NQuery;

public static class DocumentFactory
{
    private static readonly Catalog Catalog = NorthwindCatalog.Instance;

    public static AuthoringServices DefaultServices { get; } = AuthoringServices.Create();

    // The default composition with one extension point narrowed to the implementation under test,
    // which is how a per-provider test isolates its subject now that services own their providers.
    public static AuthoringServices ServicesWithOnly<TService>(TService service)
        where TService : class
    {
        return AuthoringServices.Create(b => b.AddDefaultServices()
                                              .RemoveServices<TService>()
                                              .AddService(service));
    }

    public static Document CreateQuery(string query, AuthoringServices? services = null)
    {
        return Document.Create(DocumentKind.Query, SourceText.From(query), Catalog, services ?? DefaultServices);
    }

    public static Document CreateQuery(string textWithPipe, out int position, AuthoringServices? services = null)
    {
        var text = textWithPipe.ParseSinglePosition(out position);
        return CreateQuery(text, services);
    }

    public static Document CreateQuery(string textWithMarkers, out TextSpan span, AuthoringServices? services = null)
    {
        var text = textWithMarkers.ParseSingleSpan(out span);
        return CreateQuery(text, services);
    }

    public static Document CreateQuery(string textWithMarkers, out ImmutableArray<TextSpan> spans, AuthoringServices? services = null)
    {
        var text = textWithMarkers.ParseSpans(out spans);
        return CreateQuery(text, services);
    }

    public static Document CreateExpression(string expression, AuthoringServices? services = null)
    {
        return Document.Create(DocumentKind.Expression, SourceText.From(expression), Catalog, services ?? DefaultServices);
    }

    public static Document CreateExpression(string textWithPipe, out int position, AuthoringServices? services = null)
    {
        var text = textWithPipe.ParseSinglePosition(out position);
        return CreateExpression(text, services);
    }

    public static Document CreateExpression(string textWithMarkers, out TextSpan span, AuthoringServices? services = null)
    {
        var text = textWithMarkers.ParseSingleSpan(out span);
        return CreateExpression(text, services);
    }

    public static Document CreateExpression(string textWithMarkers, out ImmutableArray<TextSpan> spans, AuthoringServices? services = null)
    {
        var text = textWithMarkers.ParseSpans(out spans);
        return CreateExpression(text, services);
    }
}
