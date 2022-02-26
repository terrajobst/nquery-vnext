﻿using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.Highlighting.Highlighters
{
    internal sealed class SelectQueryKeywordHighlighter : SelectQueryKeywordHighlighterBase<SelectQuerySyntax>
    {
        protected override IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, SelectQuerySyntax node, int position)
        {
            var hasOrderBy = node.Ancestors()
                                 .SkipWhile(n => n is ParenthesizedQuerySyntax)
                                 .FirstOrDefault() is OrderedQuerySyntax;

            return hasOrderBy
                       ? Enumerable.Empty<TextSpan>()
                       : GetSelectQueryHighlights(node);
        }
    }
}