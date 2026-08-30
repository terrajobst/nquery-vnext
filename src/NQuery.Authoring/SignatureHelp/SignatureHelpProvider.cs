using NQuery.CodeAnalysis;

namespace NQuery.Authoring.SignatureHelp;

public abstract class SignatureHelpProvider<T> : ISignatureHelpProvider
    where T : SyntaxNode
{
    public SignatureHelpResult? GetResult(DocumentView view, CancellationToken cancellationToken)
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
            : GetResult(semanticModel, node, position);
    }

    protected abstract SignatureHelpResult? GetResult(SemanticModel semanticModel, T node, int position);
}
