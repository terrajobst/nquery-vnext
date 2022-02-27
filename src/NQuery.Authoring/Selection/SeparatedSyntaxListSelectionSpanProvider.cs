using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.Selection
{
    public abstract class SeparatedSyntaxListSelectionSpanProvider<TParent, TChild> : SelectionSpanProvider<TParent>
        where TParent : SyntaxNode
        where TChild : SyntaxNode
    {
        public override IEnumerable<TextSpan> Provide(SyntaxNodeOrToken nodeOrToken, TParent parentNode)
        {
            var list = GetList(parentNode);
            var index = list.IndexOf(nodeOrToken);
            if (index >= 0)
            {
                var separator = list.GetSeparator(index);
                var node = list[index];
                var start = node.Span.Start;
                var end = separator is null ? node.Span.End : separator.Span.End;
                yield return TextSpan.FromBounds(start, end);

                var first = list.GetWithSeparators().First();
                var last = list.GetWithSeparators().Last();
                yield return TextSpan.FromBounds(first.Span.Start, last.Span.End);
            }
        }

        protected abstract SeparatedSyntaxList<TChild> GetList(TParent node);
    }
}