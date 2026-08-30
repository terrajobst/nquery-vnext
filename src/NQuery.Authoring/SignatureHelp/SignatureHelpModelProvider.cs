using NQuery.CodeAnalysis;

namespace NQuery.Authoring.SignatureHelp;

public abstract class SignatureHelpModelProvider<T> : ISignatureHelpModelProvider
    where T : SyntaxNode
{
    public SignatureHelpModel? GetModel(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);
        var position = view.Position;

        var token = semanticModel.SyntaxTree.Root.FindTokenOnLeft(position);
        var node = token.Parent?
                        .AncestorsAndSelf()
                        .OfType<T>()
                        .FirstOrDefault(c => c.IsBetweenParentheses(position));

        return node is null
            ? null
            : GetModel(semanticModel, node, position);
    }

    protected abstract SignatureHelpModel? GetModel(SemanticModel semanticModel, T node, int position);
}
