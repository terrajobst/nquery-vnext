using NQuery.Authoring.LanguageServer.Hosting;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.Northwind;

namespace NQuery.LanguageServer.Northwind;

internal sealed class NorthwindCatalogProvider : ICatalogProvider
{
    private readonly ProjectContext _context;

    public NorthwindCatalogProvider(ProjectContext context)
    {
        ThrowIfNull(context);

        _context = context;
    }

    public event EventHandler<EventArgs>? CatalogChanged;

    public async ValueTask<Catalog> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var settings = NorthwindSettings.From(_context);

        await _context.Host.LogAsync(MessageType.Info, $"Loading the Northwind catalog for project '{_context.ProjectName}'.");

        if (settings.DelayMs > 0)
            await Task.Delay(settings.DelayMs, cancellationToken);

        if (settings.Prompt)
        {
            var answer = await _context.Host.ShowMessageRequestAsync(
                MessageType.Info,
                @"Which Northwind catalog should this project use?",
                [new MessageActionItem { Title = @"Untyped" }, new MessageActionItem { Title = @"Typed" }],
                cancellationToken);

            if (answer is not null)
                settings = settings with { Typed = answer.Title == @"Typed" };
        }

        if (settings.FailWith is not null)
            throw new InvalidOperationException(settings.FailWith);

        var catalog = settings.Typed
            ? NorthwindCatalog.InstanceTyped
            : NorthwindCatalog.Instance;

        if (settings.Tables is { Length: > 0 })
            catalog = RestrictTables(catalog, settings.Tables);

        return catalog;
    }

    // Signals the server to re-resolve the catalog and re-publish diagnostics for every open
    // document -- the same path a host watching a live schema would use.
    public void Reload()
    {
        var handler = CatalogChanged;
        handler?.Invoke(this, EventArgs.Empty);
    }

    private static Catalog RestrictTables(Catalog catalog, IReadOnlyList<string> tableNames)
    {
        var keep = new HashSet<string>(tableNames, StringComparer.OrdinalIgnoreCase);
        var remove = catalog.Tables.Where(t => !keep.Contains(t.Name));
        return catalog.RemoveTables(remove);
    }
}
