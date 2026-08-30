using System.Collections.Immutable;

namespace NQuery.Authoring.SignatureHelp;

public sealed class SignatureHelpService
{
    private readonly ImmutableArray<ISignatureHelpProvider> _providers;

    public SignatureHelpService(ImmutableArray<ISignatureHelpProvider> providers)
    {
        _providers = providers;
    }

    public SignatureHelpResult? GetResult(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        return (from p in _providers
                let m = p.GetResult(view, cancellationToken)
                where m is not null
                orderby m.ApplicableSpan.Start descending
                select m).FirstOrDefault();
    }
}
