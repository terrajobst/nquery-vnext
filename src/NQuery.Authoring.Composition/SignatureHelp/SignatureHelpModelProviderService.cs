using System.Collections.Immutable;
using System.ComponentModel.Composition;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.Composition.SignatureHelp;

[Export(typeof(ISignatureHelpModelProviderService))]
internal sealed class SignatureHelpModelProviderService : ISignatureHelpModelProviderService
{
    [ImportingConstructor]
    public SignatureHelpModelProviderService([ImportMany] IEnumerable<ISignatureHelpModelProvider> providers)
    {
        Providers = SignatureHelpExtensions.StandardSignatureHelpModelProviders.AddRange(providers);
    }

    public ImmutableArray<ISignatureHelpModelProvider> Providers { get; }
}
