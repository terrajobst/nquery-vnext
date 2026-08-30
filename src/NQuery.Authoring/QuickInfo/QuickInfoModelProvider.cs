using NQuery.CodeAnalysis;

namespace NQuery.Authoring.QuickInfo;

public abstract class QuickInfoModelProvider<T> : IQuickInfoModelProvider
    where T : SyntaxNode
{
    public QuickInfoModel? GetModel(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);
        var position = view.Position;

        return semanticModel.SyntaxTree.Root.FindNodes<T>(position)
                                            .Select(node => CreateModel(semanticModel, position, node))
                                            .FirstOrDefault();
    }

    protected abstract QuickInfoModel? CreateModel(SemanticModel semanticModel, int position, T node);
}
