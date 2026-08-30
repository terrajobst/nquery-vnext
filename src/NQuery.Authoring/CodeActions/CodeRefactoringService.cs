using System.Collections.Immutable;

namespace NQuery.Authoring.CodeActions;

public sealed class CodeRefactoringService
{
    private readonly ImmutableArray<ICodeRefactoringProvider> _providers;

    public CodeRefactoringService(ImmutableArray<ICodeRefactoringProvider> providers)
    {
        _providers = providers;
    }

    public ImmutableArray<ICodeAction> GetRefactorings(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        return [.. _providers.SelectMany(p => p.GetRefactorings(view, cancellationToken))];
    }
}
