using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Selection;

// Walks out from the selection and asks the derived provider about every level whose parent is a T,
// which is the same shape BraceMatcher uses: the interface speaks documents, the walk lives here,
// and the derived type only sees the node it cares about.
public abstract class SelectionSpanProvider<T> : ISelectionSpanProvider
    where T : SyntaxNode
{
    public IEnumerable<TextSpan> Provide(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var root = view.Document.GetSyntaxTree(cancellationToken).Root;
        var token = root.FindToken(view.Selection.Start).GetPreviousTokenIfEndOfFile();

        return from nodeOrToken in GetSelfAndAncestors(token)
               where nodeOrToken.Parent is T
               from span in Provide(nodeOrToken, (T)nodeOrToken.Parent!)
               select span;
    }

    protected abstract IEnumerable<TextSpan> Provide(SyntaxNodeOrToken nodeOrToken, T parentNode);

    private static IEnumerable<SyntaxNodeOrToken> GetSelfAndAncestors(SyntaxToken token)
    {
        yield return token;

        for (var node = token.Parent; node is not null; node = node.Parent)
            yield return node;
    }
}
