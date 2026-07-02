using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Selection;

public abstract class SelectionSpanProvider<T> : ISelectionSpanProvider
    where T : SyntaxNode
{
    public IEnumerable<TextSpan> Provide(SyntaxNodeOrToken nodeOrToken)
    {
        if (nodeOrToken.Parent is not T parent)
            return [];

        return Provide(nodeOrToken, parent);
    }

    public abstract IEnumerable<TextSpan> Provide(SyntaxNodeOrToken nodeOrToken, T parentNode);
}
