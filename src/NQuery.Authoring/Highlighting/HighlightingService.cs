using System.Collections.Immutable;

using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Highlighting;

public sealed class HighlightingService
{
    private readonly ImmutableArray<IHighlighter> _highlighters;

    internal HighlightingService(ImmutableArray<IHighlighter> highlighters)
    {
        _highlighters = highlighters;
    }

    public ImmutableArray<TextSpan> GetHighlights(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);

        var result = ImmutableArray.CreateBuilder<TextSpan>();

        foreach (var highlighter in _highlighters)
            result.AddRange(highlighter.GetHighlights(semanticModel, view.Position));

        return result.ToImmutable();
    }
}
