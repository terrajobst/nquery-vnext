namespace NQuery.Authoring.LanguageServer.Protocol;

public static class CodeActionKind
{
    public const string QuickFix = @"quickfix";
    public const string Refactor = @"refactor";
}

public sealed record CodeActionOptions
{
    public IReadOnlyList<string>? CodeActionKinds { get; init; }
    public bool? ResolveProvider { get; init; }
}

public sealed record CodeActionParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
    public required Range Range { get; init; }
    public CodeActionContext? Context { get; init; }
}

public sealed record CodeActionContext
{
    public IReadOnlyList<Diagnostic>? Diagnostics { get; init; }

    // When present, the client only wants actions of these kinds -- VS Code sends it for
    // "Refactor..." and "Source Action..." as opposed to the general lightbulb.
    public IReadOnlyList<string>? Only { get; init; }
}

public sealed record CodeAction
{
    public required string Title { get; init; }
    public string? Kind { get; init; }
    public IReadOnlyList<Diagnostic>? Diagnostics { get; init; }
    public bool? IsPreferred { get; init; }
    public WorkspaceEdit? Edit { get; init; }
}

public sealed record WorkspaceEdit
{
    // Keyed by document URI exactly as the client spelled it, so the client can match it back.
    public required IReadOnlyDictionary<string, TextEdit[]> Changes { get; init; }
}
