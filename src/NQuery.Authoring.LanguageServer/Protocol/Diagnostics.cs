namespace NQuery.Authoring.LanguageServer.Protocol;

public enum DiagnosticSeverity
{
    Error = 1,
    Warning = 2,
    Information = 3,
    Hint = 4
}

public enum DiagnosticTag
{
    Unnecessary = 1,
    Deprecated = 2
}

public sealed record Diagnostic
{
    public required Range Range { get; init; }
    public DiagnosticSeverity? Severity { get; init; }

    // NQuery's DiagnosticId, so users can search for and (eventually) suppress a specific rule.
    public string? Code { get; init; }

    public string? Source { get; init; }
    public required string Message { get; init; }
    public IReadOnlyList<DiagnosticTag>? Tags { get; init; }
    public IReadOnlyList<DiagnosticRelatedInformation>? RelatedInformation { get; init; }
}

public sealed record DiagnosticRelatedInformation
{
    public required Location Location { get; init; }
    public required string Message { get; init; }
}

public sealed record PublishDiagnosticsParams
{
    public required Uri Uri { get; init; }
    public int? Version { get; init; }
    public required IReadOnlyList<Diagnostic> Diagnostics { get; init; }
}
