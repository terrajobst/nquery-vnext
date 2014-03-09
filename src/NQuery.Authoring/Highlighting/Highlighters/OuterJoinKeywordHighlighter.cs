using System;
using System.Collections.Generic;

using NQuery.Syntax;

namespace NQuery.Authoring.Highlighting.Highlighters
{
    internal sealed class OuterJoinKeywordHighlighter : KeywordHighlighter<OuterJoinedTableReferenceSyntax>
    {
        protected override IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, OuterJoinedTableReferenceSyntax node, int position)
        {
            yield return node.TypeKeyword.Span;
            if (node.OuterKeyword != null)
                yield return node.OuterKeyword.Span;
            yield return node.JoinKeyword.Span;
            yield return node.OnKeyword.Span;
        }
    }
}