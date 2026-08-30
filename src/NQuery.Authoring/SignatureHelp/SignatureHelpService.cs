using System.Collections.Immutable;

namespace NQuery.Authoring.SignatureHelp;

public sealed class SignatureHelpService
{
    private readonly ImmutableArray<ISignatureHelpModelProvider> _providers;

    public SignatureHelpService(ImmutableArray<ISignatureHelpModelProvider> providers)
    {
        _providers = providers;
    }

    public SignatureHelpModel? GetModel(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        return (from p in _providers
                let m = p.GetModel(view, cancellationToken)
                where m is not null
                orderby m.ApplicableSpan.Start descending
                select m).FirstOrDefault();
    }
}
