using NQuery.Authoring.LanguageServer.Hosting;

namespace NQuery.LanguageServer.Northwind;

// Read from the opaque "settings" member of the .nqproj file. Beyond configuring the host, these
// exist so the interesting ICatalogProvider paths -- slow resolution, failure, prompting -- can be
// reproduced deliberately instead of only showing up against a real backend.
internal sealed record NorthwindSettings
{
    // Back the catalog with strongly typed records instead of untyped DataRows.
    public bool Typed { get; init; }

    // Artificial delay before the catalog resolves, to exercise requests that arrive while the
    // catalog is still loading.
    public int DelayMs { get; init; }

    // Ask a question through window/showMessageRequest during resolution.
    public bool Prompt { get; init; }

    // Fail resolution with this message; the server should degrade to syntax-only.
    public string? FailWith { get; init; }

    // Restrict the catalog to these table names, so flipping it and reloading exercises
    // CatalogChanged invalidation.
    public string[]? Tables { get; init; }

    public static NorthwindSettings From(ProjectContext context)
    {
        ThrowIfNull(context);

        return new NorthwindSettings
        {
            Typed = context.GetSetting(@"typed", false),
            DelayMs = context.GetSetting(@"delayMs", 0),
            Prompt = context.GetSetting(@"prompt", false),
            FailWith = context.GetSetting<string?>(@"failWith"),
            Tables = context.GetSetting<string[]?>(@"tables")
        };
    }
}
