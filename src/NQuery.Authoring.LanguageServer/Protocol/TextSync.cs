namespace NQuery.Authoring.LanguageServer.Protocol;

public enum TextDocumentSyncKind
{
    None = 0,
    Full = 1,
    Incremental = 2
}

public sealed record TextDocumentSyncOptions
{
    public bool? OpenClose { get; init; }
    public TextDocumentSyncKind? Change { get; init; }
}

public sealed record DidOpenTextDocumentParams
{
    public required TextDocumentItem TextDocument { get; init; }
}

public sealed record DidChangeTextDocumentParams
{
    public required VersionedTextDocumentIdentifier TextDocument { get; init; }
    public required IReadOnlyList<TextDocumentContentChangeEvent> ContentChanges { get; init; }
}

// A null Range means "the whole document was replaced" (the Full sync form). LSP requires the
// changes in ContentChanges to be applied in order, each against the result of the previous
// one -- see LspSourceTextContainer.ApplyChanges.
public sealed record TextDocumentContentChangeEvent
{
    public Range? Range { get; init; }
    public required string Text { get; init; }
}

public sealed record DidCloseTextDocumentParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
}
