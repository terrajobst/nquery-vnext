using NQuery.CodeAnalysis;

namespace NQuery.Authoring.Completion;

public abstract class CompletionProvider<T> : ICompletionProvider
    where T : SyntaxNode
{
    public IEnumerable<CompletionItem> GetItems(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);
        var position = view.Position;

        var token = semanticModel.SyntaxTree.Root.FindTokenOnLeft(position);
        var node = token.Parent!.AncestorsAndSelf()
                               .OfType<T>()
                               .FirstOrDefault();

        return node is null
                ? []
                : GetItems(semanticModel, position, node);
    }

    protected abstract IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position, T node);
}
