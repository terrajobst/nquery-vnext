using NQuery.Text;

namespace NQuery.Authoring.Selection
{
    public abstract class SelectionSpanProvider<T> : ISelectionSpanProvider
        where T : SyntaxNode
    {
        public IEnumerable<TextSpan> Provide(SyntaxNodeOrToken nodeOrToken)
        {
            var parent = nodeOrToken.Parent as T;
            if (parent == null)
                return Enumerable.Empty<TextSpan>();

            return Provide(nodeOrToken, parent);
        }

        public abstract IEnumerable<TextSpan> Provide(SyntaxNodeOrToken nodeOrToken, T parentNode);
    }
}