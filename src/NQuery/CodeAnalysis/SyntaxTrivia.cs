using System.Collections.Immutable;

using NQuery.CodeAnalysis.Syntax;
using NQuery.CodeAnalysis.Text;

namespace NQuery.CodeAnalysis;

public sealed class SyntaxTrivia
{
    private readonly SyntaxTree _syntaxTree;
    private string? _text;

    internal SyntaxTrivia(SyntaxTree syntaxTree, SyntaxKind kind, TextSpan span, StructuredTriviaSyntax? structure, IEnumerable<Diagnostic> diagnostics)
    {
        _syntaxTree = syntaxTree;
        Kind = kind;
        Span = span;
        Structure = structure;
        Diagnostics = [.. diagnostics];
    }

    public SyntaxToken? Parent => _syntaxTree?.GetParentToken(this);

    public SyntaxKind Kind { get; }

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

    public TextSpan Span { get; }

    public StructuredTriviaSyntax? Structure { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public void WriteTo(TextWriter writer)
    {
        ThrowIfNull(writer);

        // Structured trivia (skipped tokens from error recovery) carries its text
        // in the child tokens, not in Text (which is empty). Descend into the
        // structure so WriteTo/ToString faithfully reproduces the original source.
        if (Structure is not null)
            Structure.WriteTo(writer);
        else
            writer.Write(Text);
    }

    public override string ToString()
    {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}
