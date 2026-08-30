using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Highlighting;

public abstract class KeywordHighlighter<T> : IHighlighter
    where T : SyntaxNode
{
    public IEnumerable<TextSpan> GetHighlights(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);
        var position = view.Position;

        var token = semanticModel.SyntaxTree.Root.FindToken(position);

        for (var current = token.Parent; current is not null; current = current.Parent)
        {
            if (current is T node)
            {
                var textSpans = GetHighlights(semanticModel, node, position).ToImmutableArray();
                if (textSpans.Any(s => s.ContainsOrTouches(position)))
                    return textSpans;
            }
        }

        return [];
    }

    protected abstract IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, T node, int position);
}
