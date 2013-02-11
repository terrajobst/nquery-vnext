using System;
using System.Collections.Generic;

using NQuery.Syntax;

namespace NQuery.Authoring.Highlighting
{
    internal sealed class CastKeywordHighlighter : KeywordHighlighter<CastExpressionSyntax>
    {
        protected override IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, CastExpressionSyntax node, int position)
        {
            yield return node.CastKeyword.Span;
            yield return node.AsKeyword.Span;
        }
    }
}