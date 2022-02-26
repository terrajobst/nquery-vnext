﻿using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.Highlighting.Highlighters
{
    internal sealed class CaseKeywordHighlighter : KeywordHighlighter<CaseExpressionSyntax>
    {
        protected override IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, CaseExpressionSyntax node, int position)
        {
            yield return node.CaseKeyword.Span;

            foreach (var caseLabel in node.CaseLabels)
            {
                yield return caseLabel.WhenKeyword.Span;
                yield return caseLabel.ThenKeyword.Span;
            }

            if (node.ElseLabel != null)
                yield return node.ElseLabel.ElseKeyword.Span;

            yield return node.EndKeyword.Span;
        }
    }
}