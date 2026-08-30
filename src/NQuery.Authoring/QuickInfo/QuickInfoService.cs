using System.Collections.Immutable;

namespace NQuery.Authoring.QuickInfo;

public sealed class QuickInfoService
{
    private readonly ImmutableArray<IQuickInfoProvider> _providers;

    public QuickInfoService(ImmutableArray<IQuickInfoProvider> providers)
    {
        _providers = providers;
    }

    public QuickInfoResult? GetResult(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        return (from p in _providers
                let m = p.GetResult(view, cancellationToken)
                where m is not null
                select m).FirstOrDefault();
    }
}
