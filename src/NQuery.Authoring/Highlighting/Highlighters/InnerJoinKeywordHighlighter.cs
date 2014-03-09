using System;
using System.Collections.Generic;

using NQuery.Syntax;

namespace NQuery.Authoring.Highlighting.Highlighters
{
    internal sealed class InnerJoinKeywordHighlighter : KeywordHighlighter<InnerJoinedTableReferenceSyntax>
    {
        protected override IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, InnerJoinedTableReferenceSyntax node, int position)
        {
            if (node.InnerKeyword != null)
                yield return node.InnerKeyword.Span;
            yield return node.JoinKeyword.Span;
            yield return node.OnKeyword.Span;
        }
    }
}