using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Highlighting.Highlighters;

internal sealed class CastKeywordHighlighter : KeywordHighlighter<CastExpressionSyntax>
{
    protected override IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, CastExpressionSyntax node, int position)
    {
        yield return node.CastKeyword.Span;
        yield return node.AsKeyword.Span;
    }
}
