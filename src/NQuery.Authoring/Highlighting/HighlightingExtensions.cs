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

    extension(SemanticModel semanticModel)
    {
        public IEnumerable<TextSpan> GetHighlights(int position)
        {
            return semanticModel.GetHighlights(position, StandardHighlighters);
        }

        public IEnumerable<TextSpan> GetHighlights(int position, IEnumerable<IHighlighter> highlighters)
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
}
