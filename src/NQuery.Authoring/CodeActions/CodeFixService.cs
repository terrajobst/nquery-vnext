using System.Collections.Immutable;

namespace NQuery.Authoring.CodeActions;

public sealed class CodeFixService
{
    private readonly ImmutableArray<ICodeFixProvider> _providers;

    public CodeFixService(ImmutableArray<ICodeFixProvider> providers)
    {
        _providers = providers;
    }

    public ImmutableArray<ICodeAction> GetFixes(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        return [.. _providers.SelectMany(p => p.GetFixes(view, cancellationToken))];
    }
}
