using System.Collections.Immutable;

using NQuery.CodeAnalysis.Text;

namespace NQuery.CodeAnalysis;

public sealed class SyntaxToken
{
    private readonly SyntaxTree _syntaxTree;
    private string? _text;

    internal SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, SyntaxKind contextualKind, bool isMissing, TextSpan span, object? value, IEnumerable<SyntaxTrivia> leadingTrivia, IEnumerable<SyntaxTrivia> trailingTrivia, IEnumerable<Diagnostic> diagnostics)
    {
        _syntaxTree = syntaxTree;
        Kind = kind;
        ContextualKind = contextualKind;
        IsMissing = isMissing;
        Span = span;
        Value = value;
        LeadingTrivia = [.. leadingTrivia];
        TrailingTrivia = [.. trailingTrivia];
        Diagnostics = [.. diagnostics];
    }

    public SyntaxNode? Parent => _syntaxTree?.GetParentNode(this);

    public SyntaxKind Kind { get; }

    public SyntaxKind ContextualKind { get; }

    public bool IsMissing { get; }

    public string Text
    {
        get
        {
            if (_text is null)
            {
                var text = _syntaxTree.Text.GetText(Span);
                Interlocked.CompareExchange(ref _text, text, null);
            }

            return _text;
        }
    }

    public object? Value { get; }

    public string ValueText => Value as string ?? Text;

    public TextSpan Span { get; }

    public TextSpan FullSpan
    {
        get
        {
            var start = LeadingTrivia.Length == 0
                            ? Span.Start
                            : LeadingTrivia[0].Span.Start;
            var end = TrailingTrivia.Length == 0
                          ? Span.End
                          : TrailingTrivia[TrailingTrivia.Length - 1].Span.End;
            return TextSpan.FromBounds(start, end);
        }
    }

    public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }

    public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public SyntaxToken? GetPreviousToken(bool includeZeroLength = false, bool includeSkippedTokens = false)
    {
        return SyntaxTreeNavigation.GetPreviousToken(this, includeZeroLength, includeSkippedTokens);
    }

    public SyntaxToken? GetNextToken(bool includeZeroLength = false, bool includeSkippedTokens = false)
    {
        return SyntaxTreeNavigation.GetNextToken(this, includeZeroLength, includeSkippedTokens);
    }

    public void WriteTo(TextWriter writer)
    {
        ThrowIfNull(writer);

        foreach (var syntaxTrivia in LeadingTrivia)
            syntaxTrivia.WriteTo(writer);

        writer.Write(_syntaxTree.Text.GetText(Span));

        foreach (var syntaxTrivia in TrailingTrivia)
            syntaxTrivia.WriteTo(writer);
    }

    public bool IsEquivalentTo(SyntaxToken other)
    {
        ThrowIfNull(other);

        return SyntaxTreeEquivalence.AreEquivalent(this, other);
    }

    public SyntaxToken WithDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        ThrowIfNull(diagnostics);

        return new SyntaxToken(_syntaxTree, Kind, ContextualKind, IsMissing, Span, Value, LeadingTrivia, TrailingTrivia, diagnostics);
    }

    public SyntaxToken WithKind(SyntaxKind kind)
    {
        return new SyntaxToken(_syntaxTree, kind, ContextualKind, IsMissing, Span, Value, LeadingTrivia, TrailingTrivia, Diagnostics);
    }

    public SyntaxToken WithLeadingTrivia(IEnumerable<SyntaxTrivia> trivia)
    {
        ThrowIfNull(trivia);

        return new SyntaxToken(_syntaxTree, Kind, ContextualKind, IsMissing, Span, Value, trivia, TrailingTrivia, Diagnostics);
    }

    public SyntaxToken WithTrailingTrivia(IEnumerable<SyntaxTrivia> trivia)
    {
        ThrowIfNull(trivia);

        return new SyntaxToken(_syntaxTree, Kind, ContextualKind, IsMissing, Span, Value, LeadingTrivia, trivia, Diagnostics);
    }

    public override string ToString()
    {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}
