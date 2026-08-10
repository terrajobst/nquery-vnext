using System.Collections.Immutable;

namespace NQuery.Authoring.CodeActions;

public sealed class CodeRefactoringService
{
    private readonly ImmutableArray<ICodeRefactoringProvider> _providers;

    internal CodeRefactoringService(ImmutableArray<ICodeRefactoringProvider> providers)
    {
        _providers = providers;
    }

    public ImmutableArray<ICodeAction> GetRefactorings(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);
        return [.. _providers.SelectMany(p => p.GetRefactorings(semanticModel, view.Position))];
    }
}
