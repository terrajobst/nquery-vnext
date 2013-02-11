using System.Collections.Generic;

namespace NQuery.Authoring.Highlighting
{
    public static class HighlightingExtensions
    {
        private static IEnumerable<IHighlighter> GetDefaultHighlighters()
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
            var highlighters = GetDefaultHighlighters();
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