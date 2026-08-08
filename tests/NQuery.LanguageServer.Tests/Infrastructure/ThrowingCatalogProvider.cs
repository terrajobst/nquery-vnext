using NQuery.Authoring.LanguageServer.Hosting;

namespace NQuery.LanguageServer.Infrastructure;

// Stands in for a host whose backend is unreachable.
internal sealed class ThrowingCatalogProvider : ICatalogProvider
{
    private readonly string _message;

    public ThrowingCatalogProvider(string message)
    {
        _message = message;
    }

    public ValueTask<Catalog> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(_message);
    }

    public event EventHandler<EventArgs>? CatalogChanged
    {
        add { }
        remove { }
    }
}
