using NQuery.CodeAnalysis;

namespace NQuery.Authoring.QuickInfo;

public abstract class QuickInfoProvider<T> : IQuickInfoProvider
    where T : SyntaxNode
{
    public QuickInfoResult? GetResult(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);
        var position = view.Position;

        return semanticModel.SyntaxTree.Root.FindNodes<T>(position)
                                            .Select(node => CreateResult(semanticModel, position, node))
                                            .FirstOrDefault();
    }

    protected abstract QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, T node);
}
