using NQuery.Authoring.Formatting;
using NQuery.Authoring.LanguageServer.Hosting;

namespace NQuery.Authoring.LanguageServer;

public sealed class NQueryLanguageServerOptions
{
    // Called once at initialize, after the project's settings blob has arrived -- which is why
    // this is a factory rather than an instance. Required.
    public Func<ProjectContext, ICatalogProvider>? CatalogProviderFactory { get; set; }

    // The language services every document in this server is analyzed with. A host that ships its
    // own providers builds its own composition; everything else here is server policy, which is
    // deliberately not part of the service set.
    public AuthoringServices Services { get; set; } = CreateDefaultServices();

    // The SQL layout the server formats with. LSP lets the client send only tab size, spaces vs
    // tabs, and the final newline, so everything else about style is decided here.
    public Formatting.FormattingOptions FormattingOptions { get; set; } = Formatting.FormattingOptions.Default;

    public string ServerName { get; set; } = @"NQuery Language Server";

    public string? ServerVersion { get; set; }

    // How long to wait after the last keystroke before recomputing diagnostics.
    public TimeSpan DiagnosticsDelay { get; set; } = TimeSpan.FromMilliseconds(300);

    // Running a query executes against whatever the catalog is backed by. A host serving a
    // production system, or a catalog that carries schema without data, should turn this off;
    // the client hides the Run Query command when it is disabled.
    public bool AllowExecution { get; set; } = true;

    // Hard cap on rows returned to the client. A client may request fewer, never more. Unlimited
    // by default: the client pages through the result rather than rendering all of it, so a cap
    // would only cost rows the user asked for. A host whose tables do not fit in memory, or one
    // that wants to bound response size, should set this.
    public int MaxRows { get; set; } = int.MaxValue;

    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    // The default composition plus the EditorConfig resolver. A language server is the one host
    // that knows where a document lives, which is what makes it the one that can let a repository's
    // own .editorconfig have a say -- so the file reading the authoring layer refuses to do by
    // default is opted into here. A host supplying its own Services opts in the same way.
    private static AuthoringServices CreateDefaultServices()
    {
        return AuthoringServices.Create(builder =>
        {
            builder.AddDefaultServices();
            builder.RemoveServices<FormattingOptionsResolver>();
            builder.AddService<FormattingOptionsResolver, EditorConfigFormattingOptionsResolver>();
        });
    }
}
