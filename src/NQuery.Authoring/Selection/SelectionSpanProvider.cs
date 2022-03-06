using NQuery.Text;

namespace NQuery.Authoring.Selection
{
    public abstract class SelectionSpanProvider<T> : ISelectionSpanProvider
        where T : SyntaxNode
    {
        public IEnumerable<TextSpan> Provide(SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.Parent is not T parent)
                return Enumerable.Empty<TextSpan>();

            return Provide(nodeOrToken, parent);
        }

        public abstract IEnumerable<TextSpan> Provide(SyntaxNodeOrToken nodeOrToken, T parentNode);
    }
}