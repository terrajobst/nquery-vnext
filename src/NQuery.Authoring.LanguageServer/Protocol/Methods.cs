namespace NQuery.Authoring.LanguageServer.Protocol;

// LSP method names. Kept as constants so handler registration and tests agree on spelling.
public static class Methods
{
    // Lifecycle
    public const string Initialize = @"initialize";
    public const string Initialized = @"initialized";
    public const string Shutdown = @"shutdown";
    public const string Exit = @"exit";

    // Text synchronization
    public const string TextDocumentDidOpen = @"textDocument/didOpen";
    public const string TextDocumentDidChange = @"textDocument/didChange";
    public const string TextDocumentDidClose = @"textDocument/didClose";

    // Diagnostics
    public const string TextDocumentPublishDiagnostics = @"textDocument/publishDiagnostics";

    // Language features
    public const string TextDocumentCompletion = @"textDocument/completion";
    public const string TextDocumentHover = @"textDocument/hover";
    public const string TextDocumentSignatureHelp = @"textDocument/signatureHelp";
    public const string TextDocumentDefinition = @"textDocument/definition";
    public const string TextDocumentReferences = @"textDocument/references";
    public const string TextDocumentDocumentHighlight = @"textDocument/documentHighlight";
    public const string TextDocumentSemanticTokensFull = @"textDocument/semanticTokens/full";
    public const string TextDocumentFoldingRange = @"textDocument/foldingRange";
    public const string TextDocumentSelectionRange = @"textDocument/selectionRange";
    public const string TextDocumentCodeAction = @"textDocument/codeAction";

    // Window
    public const string WindowShowMessage = @"window/showMessage";
    public const string WindowShowMessageRequest = @"window/showMessageRequest";
    public const string WindowLogMessage = @"window/logMessage";

    // Workspace
    public const string WorkspaceConfiguration = @"workspace/configuration";

    // NQuery extensions
    public const string NQueryReloadCatalog = @"nquery/reloadCatalog";
    public const string NQueryExecute = @"nquery/execute";
    public const string NQueryShowPlan = @"nquery/showPlan";

    // Server -> client notification, so the client can show a persistent indicator rather than
    // relying on a transient window/showMessage toast the user may never see.
    public const string NQueryCatalogStatus = @"nquery/catalogStatus";
}
