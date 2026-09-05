using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Formatting;

// Document in, Document out, like CommentingService -- with the underlying changes exposed too,
// because the editor protocols want edits rather than a document.
//
// Options are a parameter rather than state: this is a stateless singleton shared by every
// document, and a host resolves whatever settings it has (an editor's per-request values, a config
// file) before asking.
public sealed class FormattingService
{
    public Document Format(Document document, CancellationToken cancellationToken = default)
    {
        return Format(document, FormattingOptions.Default, cancellationToken);
    }

    public Document Format(Document document, FormattingOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);
        ThrowIfNull(options);

        var changes = GetChanges(document, options, cancellationToken);
        return ApplyChanges(document, changes);
    }

    public Document Format(Document document, TextSpan span, FormattingOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);
        ThrowIfNull(options);

        var changes = GetChanges(document, span, options, cancellationToken);
        return ApplyChanges(document, changes);
    }

    public ImmutableArray<TextChange> GetChanges(Document document, FormattingOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);
        ThrowIfNull(options);

        var syntaxTree = document.GetSyntaxTree(cancellationToken);
        return Formatter.GetChanges(syntaxTree, options);
    }

    // The whole document is still formatted -- indentation inside a span depends on everything
    // enclosing it -- and only the changes that reach the span are kept.
    public ImmutableArray<TextChange> GetChanges(Document document, TextSpan span, FormattingOptions options, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);
        ThrowIfNull(options);

        var changes = GetChanges(document, options, cancellationToken);
        return [.. changes.Where(c => c.Span.IntersectsWith(span))];
    }

    private static Document ApplyChanges(Document document, ImmutableArray<TextChange> changes)
    {
        if (changes.Length == 0)
            return document;

        return document.WithText(document.Text.WithChanges(changes));
    }
}
