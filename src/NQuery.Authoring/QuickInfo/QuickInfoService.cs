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

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);

        return (from p in _providers
                let m = p.GetModel(semanticModel, view.Position)
                where m is not null
                select m).FirstOrDefault();
    }
}
