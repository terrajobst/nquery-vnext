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
    public TextSpan ExtendSelection(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        var syntaxTree = view.Document.GetSyntaxTree(cancellationToken);
        var selectedSpan = view.Selection;

        var token = syntaxTree.Root.FindToken(selectedSpan.Start).GetPreviousTokenIfEndOfFile();
        foreach (var span in GetNextSpans(token))
        {
            if (!selectedSpan.Contains(span))
                return span;
        }

        var node = token.Parent;
        while (node is not null)
        {
            foreach (var span in GetNextSpans(node))
            {
                if (!selectedSpan.Contains(span))
                    return span;
            }

            node = node.Parent;
        }

        return syntaxTree.Root.Span;
    }

    private IEnumerable<TextSpan> GetNextSpans(SyntaxNodeOrToken nodeOrToken)
    {
        yield return nodeOrToken.Span;

        var spans = _providers.SelectMany(p => p.Provide(nodeOrToken));
        foreach (var span in spans)
            yield return span;
    }
}
