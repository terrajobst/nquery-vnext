namespace NQuery.Authoring.LanguageServer.Protocol;

// Custom NQuery requests. These are not part of LSP; the server advertises them under
// ServerCapabilities.Experimental so a client can hide the commands when a host disables them.

public sealed record ExecuteParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }

    // Overrides the server's configured cap when smaller. Never allowed to raise it.
    public int? MaxRows { get; init; }
}

public sealed record ExecuteResult
{
    public required IReadOnlyList<ResultColumn> Columns { get; init; }

    // Cells are pre-rendered display strings; null means SQL NULL. Sending them as JSON values
    // instead would lose decimal precision and turn a byte[] column into megabytes of base64 --
    // see ValueFormatter.
    public required IReadOnlyList<IReadOnlyList<string?>> Rows { get; init; }

    public required bool Truncated { get; init; }
    public required long ElapsedMilliseconds { get; init; }

    // Set when the query could not be run; Columns/Rows are empty in that case.
    public string? ErrorMessage { get; init; }
}

public sealed record ResultColumn
{
    public required string Name { get; init; }
    public required string Type { get; init; }
}

public sealed record CatalogStatusParams
{
    public required string ProjectName { get; init; }
    public required bool Available { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed record ShowPlanParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
}

public sealed record ShowPlanResult
{
    // The whole pipeline: the unoptimized logical tree, one entry per optimization pass that
    // changed it, the optimized tree, and the physical plan.
    public required IReadOnlyList<ShowPlanStep> Steps { get; init; }

    public string? ErrorMessage { get; init; }
}

public sealed record ShowPlanStep
{
    public required string Name { get; init; }
    public required ShowPlanNodeInfo Root { get; init; }
}

public sealed record ShowPlanNodeInfo
{
    public required string OperatorName { get; init; }
    public required bool IsScalar { get; init; }
    public required IReadOnlyList<ShowPlanProperty> Properties { get; init; }
    public required IReadOnlyList<ShowPlanNodeInfo> Children { get; init; }
}

public sealed record ShowPlanProperty
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}
