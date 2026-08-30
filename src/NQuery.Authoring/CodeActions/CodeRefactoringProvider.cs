using NQuery.CodeAnalysis;

namespace NQuery.Authoring.CodeActions;

public abstract class CodeRefactoringProvider<T> : ICodeRefactoringProvider
    where T : SyntaxNode
{
    public IEnumerable<ICodeAction> GetRefactorings(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);
        var position = view.Position;

        return semanticModel.SyntaxTree.Root.FindNodes<T>(position)
                                            .SelectMany(n => GetRefactorings(semanticModel, position, n));
    }

    protected abstract IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, T node);
}
