using System.Collections.Immutable;

namespace NQuery.Authoring.QuickInfo;

public sealed class QuickInfoService
{
    private readonly ImmutableArray<IQuickInfoModelProvider> _providers;

    public QuickInfoService(ImmutableArray<IQuickInfoModelProvider> providers)
    {
        _providers = providers;
    }

    public QuickInfoModel? GetModel(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        return (from p in _providers
                let m = p.GetModel(view, cancellationToken)
                where m is not null
                select m).FirstOrDefault();
    }
}
