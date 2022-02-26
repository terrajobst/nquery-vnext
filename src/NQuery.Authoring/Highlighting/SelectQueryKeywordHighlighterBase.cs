using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.Highlighting
{
    internal abstract class SelectQueryKeywordHighlighterBase<T> : KeywordHighlighter<T>
        where T : SyntaxNode
    {
        protected IEnumerable<TextSpan> GetSelectQueryHighlights(SelectQuerySyntax node)
        {
            yield return node.SelectClause.SelectKeyword.Span;

            if (node.FromClause != null)
                yield return node.FromClause.FromKeyword.Span;

            if (node.WhereClause != null)
                yield return node.WhereClause.WhereKeyword.Span;

            if (node.GroupByClause != null)
                yield return TextSpan.FromBounds(node.GroupByClause.GroupKeyword.Span.Start,
                                                 node.GroupByClause.ByKeyword.Span.End);

            if (node.HavingClause != null)
                yield return node.HavingClause.HavingKeyword.Span;
        }
    }
}