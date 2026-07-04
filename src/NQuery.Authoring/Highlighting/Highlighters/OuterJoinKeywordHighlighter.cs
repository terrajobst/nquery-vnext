using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Highlighting.Highlighters;

internal sealed class OuterJoinKeywordHighlighter : KeywordHighlighter<OuterJoinedTableReferenceSyntax>
{
    protected override IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, OuterJoinedTableReferenceSyntax node, int position)
    {
        yield return node.JoinTypeKeyword.Span;
        if (node.OuterKeyword is not null)
            yield return node.OuterKeyword.Span;
        yield return node.JoinKeyword.Span;
        yield return node.OnKeyword.Span;
    }
}
