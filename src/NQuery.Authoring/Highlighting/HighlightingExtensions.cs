using System.Collections.Generic;

using NQuery.Authoring.Highlighting.Highlighters;
using NQuery.Text;

namespace NQuery.Authoring.Highlighting
{
    public static class HighlightingExtensions
    {
        public static IEnumerable<IHighlighter> GetStandardHighlighters()
        {
            return new IHighlighter[]
                       {
                           new CaseKeywordHighlighter(),
                           new CastKeywordHighlighter(),
                           new SelectQueryKeywordHighlighter(),
                           new OrderedQueryKeywordHighlighter(),
                           new InnerJoinKeywordHighlighter(),
                           new OuterJoinKeywordHighlighter(),
                           new SymbolReferenceHighlighter()
                       };
        }

        public static IEnumerable<TextSpan> GetHighlights(this SemanticModel semanticModel, int position)
        {
            var highlighters = GetStandardHighlighters();
            return semanticModel.GetHighlights(position, highlighters);
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
}