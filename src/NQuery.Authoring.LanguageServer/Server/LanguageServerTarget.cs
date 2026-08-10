using System.Text.Json;

using NQuery.Authoring.LanguageServer.Documents;
using NQuery.Authoring.LanguageServer.Hosting;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis;

using StreamJsonRpc;

namespace NQuery.Authoring.LanguageServer.Server;

// The JSON-RPC target: every [JsonRpcMethod] here is an LSP entry point. It also implements
// ILanguageServerHost, which is what the app-specific catalog provider talks back through.
internal sealed partial class LanguageServerTarget : ILanguageServerHost
{
    private readonly NQueryLanguageServerOptions _options;
    private readonly DocumentStore _documents;
    private readonly TaskCompletionSource _exited = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly object _catalogGate = new();

    private JsonRpc? _rpc;
    private ProjectContext? _project;
    private ICatalogProvider? _catalogProvider;
    private Task<Catalog>? _catalogTask;
    private Catalog? _appliedCatalog;
    private string? _catalogError;
    private bool _supportsConfiguration;

    public LanguageServerTarget(NQueryLanguageServerOptions options)
    {
        ThrowIfNull(options);

        _options = options;
        _documents = new DocumentStore(options.Services);
    }

    public Task Exited
    {
        get { return _exited.Task; }
    }

    public void Attach(JsonRpc rpc)
    {
        ThrowIfNull(rpc);

        _rpc = rpc;
    }

    private JsonRpc Rpc
    {
        get { return _rpc ?? throw new InvalidOperationException(@"The server is not attached to a JSON-RPC connection."); }
    }

    // Lifecycle

    [JsonRpcMethod(Methods.Initialize, UseSingleObjectParameterDeserialization = true)]
    public InitializeResult Initialize(InitializeParams parameters)
    {
        ThrowIfNull(parameters);

        if (_options.CatalogProviderFactory is null)
            throw new InvalidOperationException(@"NQueryLanguageServerOptions.CatalogProviderFactory must be set.");

        var options = parameters.InitializationOptions ?? default;
        var projectFile = TryGetUri(options, @"projectFile");
        var projectName = TryGetString(options, @"projectName") ?? @"NQuery";
        var settings = TryGetElement(options, @"settings");

        _supportsConfiguration = parameters.Capabilities?.Workspace?.Configuration ?? false;

        _project = new ProjectContext(projectFile, projectName, settings, this);
        _catalogProvider = _options.CatalogProviderFactory(_project);
        _catalogProvider.CatalogChanged += CatalogProviderOnCatalogChanged;

        // Resolution is kicked off in `initialized` rather than here so a slow catalog never
        // delays the handshake; requests arriving before it completes await it (EnsureCatalogAsync).
        return new InitializeResult
        {
            ServerInfo = new ServerInfo { Name = _options.ServerName, Version = _options.ServerVersion },
            Capabilities = CreateCapabilities()
        };
    }

    [JsonRpcMethod(Methods.Initialized, UseSingleObjectParameterDeserialization = true)]
    public void Initialized(JsonElement parameters)
    {
        _ = EnsureCatalogAsync(CancellationToken.None);
    }

    [JsonRpcMethod(Methods.Shutdown)]
    public object? Shutdown()
    {
        return null;
    }

    [JsonRpcMethod(Methods.Exit)]
    public void Exit()
    {
        _exited.TrySetResult();
    }

    [JsonRpcMethod(Methods.NQueryReloadCatalog)]
    public async Task ReloadCatalogAsync(CancellationToken cancellationToken)
    {
        InvalidateCatalog();

        // Explicit, because republishing only touches open documents -- with none open the
        // catalog would never be re-resolved and the client would keep showing the stale failure.
        await EnsureCatalogAsync(cancellationToken);
        await RepublishAllDiagnosticsAsync(cancellationToken);
    }

    private ServerCapabilities CreateCapabilities()
    {
        return new ServerCapabilities
        {
            PositionEncoding = PositionEncodingKind.Utf16,
            TextDocumentSync = new TextDocumentSyncOptions
            {
                OpenClose = true,
                Change = TextDocumentSyncKind.Incremental
            },
            CompletionProvider = new CompletionOptions
            {
                // '.' opens property/method and qualified-column completion; the others let a
                // list appear right after typing a separator instead of needing another keystroke.
                TriggerCharacters = [@".", @" ", @",", @"("],
                ResolveProvider = false
            },
            HoverProvider = true,
            SignatureHelpProvider = new SignatureHelpOptions
            {
                TriggerCharacters = [@"(", @","],
                RetriggerCharacters = [@","]
            },
            DefinitionProvider = true,
            ReferencesProvider = true,
            DocumentHighlightProvider = true,
            SemanticTokensProvider = new SemanticTokensOptions
            {
                Legend = Mapping.ClassificationMapping.Legend,
                Full = true
            },
            FoldingRangeProvider = true,
            SelectionRangeProvider = true,
            CodeActionProvider = new CodeActionOptions
            {
                CodeActionKinds = [Protocol.CodeActionKind.QuickFix, Protocol.CodeActionKind.Refactor],

                // Edits are computed up front; see LanguageServerTarget.CodeActions.cs for when
                // that should become a resolve instead.
                ResolveProvider = false
            },
            Experimental = new ExperimentalCapabilities
            {
                Execute = _options.AllowExecution,
                ShowPlan = true,
                MaxRows = _options.MaxRows
            }
        };
    }

    // Catalog

    private void CatalogProviderOnCatalogChanged(object? sender, EventArgs e)
    {
        InvalidateCatalog();
        _ = RepublishAllDiagnosticsAsync(CancellationToken.None);
    }

    // Non-null when the last resolution attempt failed. Everything that needs a catalog reports
    // this instead of pretending the catalog is merely empty.
    internal string? CatalogError
    {
        get
        {
            lock (_catalogGate)
                return _catalogError;
        }
    }

    private void InvalidateCatalog()
    {
        lock (_catalogGate)
        {
            _catalogTask = null;
            _appliedCatalog = null;
            _catalogError = null;
        }
    }

    // Resolves the catalog at most once per invalidation and pushes it into every open document.
    // Feature handlers await this before taking a snapshot, so a request that arrives while a
    // slow catalog is still loading answers against the real catalog rather than Catalog.Empty.
    private async Task EnsureCatalogAsync(CancellationToken cancellationToken)
    {
        if (_catalogProvider is null)
            return;

        Task<Catalog> task;

        lock (_catalogGate)
        {
            _catalogTask ??= ResolveCatalogAsync(_catalogProvider);
            task = _catalogTask;
        }

        var catalog = await task.WaitAsync(cancellationToken);

        // The store update happens inside the lock, and _appliedCatalog is only meaningful once
        // it has. Marking it applied first and updating afterwards let a second caller -- the
        // didOpen path racing the one started by `initialized` -- observe "already applied" while
        // the store still held Catalog.Empty, and bind the document against nothing.
        //
        // DocumentStore takes its own lock but never calls back here, so nesting is safe.
        lock (_catalogGate)
        {
            if (ReferenceEquals(_appliedCatalog, catalog))
                return;

            _documents.SetCatalog(catalog);
            _appliedCatalog = catalog;
        }
    }

    private async Task<Catalog> ResolveCatalogAsync(ICatalogProvider provider)
    {
        try
        {
            // Task.Run so a provider that throws or blocks synchronously cannot do so on the
            // caller's stack, which is inside the catalog lock.
            var catalog = await Task.Run(() => provider.GetCatalogAsync(CancellationToken.None).AsTask());

            lock (_catalogGate)
                _catalogError = null;

            await SafeNotifyCatalogStatusAsync(null);
            return catalog;
        }
        catch (Exception ex)
        {
            // A host that cannot reach its schema degrades to syntax-only rather than taking the
            // whole server down. Recording the message is what lets diagnostics, execution and
            // the client's status bar explain the situation instead of reporting every name in
            // the document as undeclared.
            lock (_catalogGate)
                _catalogError = ex.Message;

            await SafeShowMessageAsync(MessageType.Error, $"Failed to load the NQuery catalog: {ex.Message}");
            await SafeNotifyCatalogStatusAsync(ex.Message);

            return Catalog.Default;
        }
    }

    private async Task SafeNotifyCatalogStatusAsync(string? error)
    {
        try
        {
            var parameters = new CatalogStatusParams
            {
                ProjectName = _project?.ProjectName ?? @"NQuery",
                Available = error is null,
                ErrorMessage = error
            };

            await Rpc.NotifyWithParameterObjectAsync(Methods.NQueryCatalogStatus, parameters);
        }
        catch (Exception)
        {
            // Connection is gone.
        }
    }

    private async Task<DocumentSnapshot?> TryGetSnapshotAsync(Uri uri, CancellationToken cancellationToken)
    {
        await EnsureCatalogAsync(cancellationToken);

        return _documents.TryGetSnapshot(uri, out var snapshot)
            ? snapshot
            : null;
    }

    // Takes a snapshot and warms its semantic model, so that the language services -- which are
    // synchronous -- run against an already-computed binding instead of parsing on this thread.
    private async Task<Document?> TryGetBoundDocumentAsync(Uri uri, CancellationToken cancellationToken)
    {
        var snapshot = await TryGetSnapshotAsync(uri, cancellationToken);
        if (snapshot is null)
            return null;

        var document = snapshot.Value.Document;
        await document.GetSemanticModelAsync(cancellationToken);
        return document;
    }

    // ILanguageServerHost

    public Task ShowMessageAsync(MessageType type, string message, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(message);

        var parameters = new ShowMessageParams { Type = type, Message = message };
        return Rpc.NotifyWithParameterObjectAsync(Methods.WindowShowMessage, parameters);
    }

    public async Task<MessageActionItem?> ShowMessageRequestAsync(MessageType type,
                                                                  string message,
                                                                  IReadOnlyList<MessageActionItem> actions,
                                                                  CancellationToken cancellationToken = default)
    {
        ThrowIfNull(message);
        ThrowIfNull(actions);

        var parameters = new ShowMessageRequestParams { Type = type, Message = message, Actions = actions };
        return await Rpc.InvokeWithParameterObjectAsync<MessageActionItem?>(Methods.WindowShowMessageRequest, parameters, cancellationToken);
    }

    public Task LogAsync(MessageType type, string message, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(message);

        var parameters = new LogMessageParams { Type = type, Message = message };
        return Rpc.NotifyWithParameterObjectAsync(Methods.WindowLogMessage, parameters);
    }

    public async Task<JsonElement?> GetConfigurationAsync(string section, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(section);

        if (!_supportsConfiguration)
            return null;

        var parameters = new ConfigurationParams
        {
            Items = [new ConfigurationItem { Section = section }]
        };

        var result = await Rpc.InvokeWithParameterObjectAsync<JsonElement[]>(Methods.WorkspaceConfiguration, parameters, cancellationToken);
        return result is { Length: > 0 } ? result[0] : null;
    }

    private async Task SafeShowMessageAsync(MessageType type, string message)
    {
        try
        {
            await ShowMessageAsync(type, message);
        }
        catch (Exception)
        {
            // The connection may already be gone; there is nowhere left to report this.
        }
    }

    // initializationOptions helpers

    private static JsonElement TryGetElement(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
            ? value
            : default;
    }

    private static string? TryGetString(JsonElement element, string name)
    {
        var value = TryGetElement(element, name);
        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static Uri? TryGetUri(JsonElement element, string name)
    {
        var value = TryGetString(element, name);
        return value is not null && Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;
    }
}
