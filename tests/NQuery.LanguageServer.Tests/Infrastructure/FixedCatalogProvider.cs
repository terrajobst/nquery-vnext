using NQuery.Authoring.LanguageServer.Hosting;

namespace NQuery.LanguageServer.Infrastructure;

internal sealed class FixedCatalogProvider : ICatalogProvider
{
    private readonly Catalog _catalog;
    private readonly TimeSpan _delay;

    public FixedCatalogProvider(Catalog catalog, TimeSpan delay = default)
    {
        _catalog = catalog;
        _delay = delay;
    }

    public async ValueTask<Catalog> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        if (_delay > TimeSpan.Zero)
            await Task.Delay(_delay, cancellationToken);

        return _catalog;
    }

    public event EventHandler<EventArgs>? CatalogChanged
    {
        add { }
        remove { }
    }
}
