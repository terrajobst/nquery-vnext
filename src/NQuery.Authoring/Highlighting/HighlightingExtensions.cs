using System.Collections.Immutable;

using NQuery.Authoring.Highlighting.Highlighters;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Highlighting;

public static class HighlightingExtensions
{
    public static ImmutableArray<IHighlighter> StandardHighlighters { get; } =
    [
        new CaseKeywordHighlighter(),
        new CastKeywordHighlighter(),
        new SelectQueryKeywordHighlighter(),
        new OrderedQueryKeywordHighlighter(),
        new InnerJoinKeywordHighlighter(),
        new OuterJoinKeywordHighlighter(),
        new SymbolReferenceHighlighter()
    ];

    public static IEnumerable<TextSpan> GetHighlights(this SemanticModel semanticModel, int position)
    {
        return semanticModel.GetHighlights(position, StandardHighlighters);
    }

    public static IEnumerable<TextSpan> GetHighlights(this SemanticModel semanticModel, int position, IEnumerable<IHighlighter> highlighters)
    {
        var result = new List<TextSpan>();

        foreach (var highlighter in highlighters)
        {
            var highlights = highlighter.GetHighlights(semanticModel, position);
            result.AddRange(highlights);
        }

        return result;
    }
}
