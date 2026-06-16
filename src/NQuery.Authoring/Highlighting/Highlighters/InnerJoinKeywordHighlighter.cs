using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Highlighting.Highlighters;

internal sealed class InnerJoinKeywordHighlighter : KeywordHighlighter<InnerJoinedTableReferenceSyntax>
{
    protected override IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, InnerJoinedTableReferenceSyntax node, int position)
    {
        if (node.InnerKeyword is not null)
            yield return node.InnerKeyword.Span;
        yield return node.JoinKeyword.Span;
        yield return node.OnKeyword.Span;
    }
}
