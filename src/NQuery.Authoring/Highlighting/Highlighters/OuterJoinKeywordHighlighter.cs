using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.Highlighting.Highlighters
{
    internal sealed class OuterJoinKeywordHighlighter : KeywordHighlighter<OuterJoinedTableReferenceSyntax>
    {
        protected override IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, OuterJoinedTableReferenceSyntax node, int position)
        {
            yield return node.TypeKeyword.Span;
            if (node.OuterKeyword is not null)
                yield return node.OuterKeyword.Span;
            yield return node.JoinKeyword.Span;
            yield return node.OnKeyword.Span;
        }
    }
}