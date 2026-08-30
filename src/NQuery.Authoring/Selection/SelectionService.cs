using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Selection;

public sealed class SelectionService
{
    private readonly ImmutableArray<ISelectionSpanProvider> _providers;

    public SelectionService(ImmutableArray<ISelectionSpanProvider> providers)
    {
        _providers = providers;
    }

    // Widens by a single step; a caller building a chain of enclosing ranges calls this repeatedly,
    // feeding the previous result back in as the view's selection.
    //
    // Every candidate below encloses the one before it, so "the first span the selection doesn't
    // already cover" and "the smallest such span" are the same answer. Picking by size is what lets
    // a provider be asked for its whole chain at once instead of a level at a time.
    public TextSpan ExtendSelection(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        var root = view.Document.GetSyntaxTree(cancellationToken).Root;
        var selectedSpan = view.Selection;

        var candidates = GetEnclosingSpans(root, selectedSpan.Start)
                            .Concat(_providers.SelectMany(p => p.GetSpans(view, cancellationToken)));

        var result = root.Span;

        foreach (var candidate in candidates)
        {
            if (!selectedSpan.Contains(candidate) && candidate.Length < result.Length)
                result = candidate;
        }

        return result;
    }

    private static IEnumerable<TextSpan> GetEnclosingSpans(SyntaxNode root, int position)
    {
        var token = root.FindToken(position).GetPreviousTokenIfEndOfFile();
        yield return token.Span;

        for (var node = token.Parent; node is not null; node = node.Parent)
            yield return node.Span;
    }
}
