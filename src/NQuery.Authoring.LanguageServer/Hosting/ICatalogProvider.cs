namespace NQuery.Authoring.LanguageServer.Hosting;

// The one thing an app-specific host has to supply. Resolution is async because a real catalog
// usually comes from somewhere slow (a database, a service, a file the user has to pick), and
// CatalogChanged exists so a host watching a live schema can invalidate without a restart.
public interface ICatalogProvider
{
    ValueTask<Catalog> GetCatalogAsync(CancellationToken cancellationToken = default);

    event EventHandler<EventArgs>? CatalogChanged;
}
