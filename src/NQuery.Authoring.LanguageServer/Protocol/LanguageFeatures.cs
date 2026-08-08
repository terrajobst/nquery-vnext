namespace NQuery.Authoring.LanguageServer.Protocol;

// Hover

public sealed record HoverParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
    public required Position Position { get; init; }
}

public sealed record Hover
{
    public required MarkupContent Contents { get; init; }
    public Range? Range { get; init; }
}

// Signature help

public sealed record SignatureHelpOptions
{
    public IReadOnlyList<string>? TriggerCharacters { get; init; }
    public IReadOnlyList<string>? RetriggerCharacters { get; init; }
}

public sealed record SignatureHelpParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
    public required Position Position { get; init; }
}

public sealed record SignatureHelp
{
    public required IReadOnlyList<SignatureInformation> Signatures { get; init; }
    public int? ActiveSignature { get; init; }
    public int? ActiveParameter { get; init; }
}

public sealed record SignatureInformation
{
    public required string Label { get; init; }
    public MarkupContent? Documentation { get; init; }
    public IReadOnlyList<ParameterInformation>? Parameters { get; init; }
}

// NQuery's ParameterItem carries a TextSpan into the signature's own text, which maps onto
// LSP's offset-pair label form exactly -- no substring matching needed.
public sealed record ParameterInformation
{
    public required int[] Label { get; init; }
    public MarkupContent? Documentation { get; init; }
}

// Definition and references

public sealed record DefinitionParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
    public required Position Position { get; init; }
}

public sealed record ReferenceParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
    public required Position Position { get; init; }
    public ReferenceContext? Context { get; init; }
}

public sealed record ReferenceContext
{
    public required bool IncludeDeclaration { get; init; }
}

// Document highlight

public enum DocumentHighlightKind
{
    Text = 1,
    Read = 2,
    Write = 3
}

public sealed record DocumentHighlightParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
    public required Position Position { get; init; }
}

public sealed record DocumentHighlight
{
    public required Range Range { get; init; }
    public DocumentHighlightKind? Kind { get; init; }
}

// Folding ranges

public sealed record FoldingRangeParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
}

public sealed record FoldingRange
{
    public required int StartLine { get; init; }
    public int? StartCharacter { get; init; }
    public required int EndLine { get; init; }
    public int? EndCharacter { get; init; }
    public string? Kind { get; init; }
    public string? CollapsedText { get; init; }
}

public static class FoldingRangeKind
{
    public const string Comment = @"comment";
    public const string Imports = @"imports";
    public const string Region = @"region";
}

// Selection ranges

public sealed record SelectionRangeParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
    public required IReadOnlyList<Position> Positions { get; init; }
}

public sealed record SelectionRange
{
    public required Range Range { get; init; }
    public SelectionRange? Parent { get; init; }
}

// Semantic tokens

public sealed record SemanticTokensOptions
{
    public required SemanticTokensLegend Legend { get; init; }
    public bool? Full { get; init; }
}

public sealed record SemanticTokensLegend
{
    public required IReadOnlyList<string> TokenTypes { get; init; }
    public required IReadOnlyList<string> TokenModifiers { get; init; }
}

public sealed record SemanticTokensParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
}

public sealed record SemanticTokens
{
    public string? ResultId { get; init; }
    public required int[] Data { get; init; }
}
