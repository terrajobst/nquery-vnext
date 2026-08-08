using System.Text.Json;
using System.Text.Json.Serialization;

namespace NQuery.Authoring.LanguageServer.Protocol;

public sealed record InitializeParams
{
    public int? ProcessId { get; init; }
    public ClientInfo? ClientInfo { get; init; }
    public Uri? RootUri { get; init; }

    // The opaque `settings` blob from the .nqproj file, forwarded verbatim by the client.
    public JsonElement? InitializationOptions { get; init; }

    public ClientCapabilities? Capabilities { get; init; }
    public IReadOnlyList<WorkspaceFolder>? WorkspaceFolders { get; init; }
}

public sealed record ClientInfo
{
    public required string Name { get; init; }
    public string? Version { get; init; }
}

public sealed record WorkspaceFolder
{
    public required Uri Uri { get; init; }
    public required string Name { get; init; }
}

public sealed record ClientCapabilities
{
    public GeneralClientCapabilities? General { get; init; }
    public TextDocumentClientCapabilities? TextDocument { get; init; }
    public WorkspaceClientCapabilities? Workspace { get; init; }
}

public sealed record WorkspaceClientCapabilities
{
    // Whether the client answers workspace/configuration pull requests.
    public bool? Configuration { get; init; }
}

public sealed record GeneralClientCapabilities
{
    public IReadOnlyList<string>? PositionEncodings { get; init; }
}

public sealed record TextDocumentClientCapabilities
{
    public CompletionClientCapabilities? Completion { get; init; }
    public HoverClientCapabilities? Hover { get; init; }
}

public sealed record CompletionClientCapabilities
{
    public CompletionItemClientCapabilities? CompletionItem { get; init; }
}

public sealed record CompletionItemClientCapabilities
{
    public IReadOnlyList<MarkupKind>? DocumentationFormat { get; init; }
    public bool? SnippetSupport { get; init; }
}

public sealed record HoverClientCapabilities
{
    public IReadOnlyList<MarkupKind>? ContentFormat { get; init; }
}

public sealed record InitializeResult
{
    public required ServerCapabilities Capabilities { get; init; }
    public ServerInfo? ServerInfo { get; init; }
}

public sealed record ServerInfo
{
    public required string Name { get; init; }
    public string? Version { get; init; }
}

// .NET strings are UTF-16, so utf-16 offsets map to SourceText positions with no conversion.
// This is also LSP's default, so a client that omits general.positionEncodings gets it anyway.
public static class PositionEncodingKind
{
    public const string Utf8 = @"utf-8";
    public const string Utf16 = @"utf-16";
    public const string Utf32 = @"utf-32";
}

public sealed record ServerCapabilities
{
    public string? PositionEncoding { get; init; }
    public TextDocumentSyncOptions? TextDocumentSync { get; init; }
    public CompletionOptions? CompletionProvider { get; init; }
    public bool? HoverProvider { get; init; }
    public SignatureHelpOptions? SignatureHelpProvider { get; init; }
    public bool? DefinitionProvider { get; init; }
    public bool? ReferencesProvider { get; init; }
    public bool? DocumentHighlightProvider { get; init; }
    public SemanticTokensOptions? SemanticTokensProvider { get; init; }
    public bool? FoldingRangeProvider { get; init; }
    public bool? SelectionRangeProvider { get; init; }
    public CodeActionOptions? CodeActionProvider { get; init; }

    // Non-standard NQuery capabilities (nquery/execute, nquery/showPlan) go here so a client can
    // discover them without guessing.
    public ExperimentalCapabilities? Experimental { get; init; }
}

public sealed record ExperimentalCapabilities
{
    public required bool Execute { get; init; }
    public required bool ShowPlan { get; init; }
    public required int MaxRows { get; init; }
}

public sealed record ConfigurationParams
{
    public required IReadOnlyList<ConfigurationItem> Items { get; init; }
}

public sealed record ConfigurationItem
{
    public Uri? ScopeUri { get; init; }
    public string? Section { get; init; }
}
