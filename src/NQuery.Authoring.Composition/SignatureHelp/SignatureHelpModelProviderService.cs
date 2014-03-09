using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.Composition.SignatureHelp
{
    [Export(typeof(ISignatureHelpModelProviderService))]
    internal sealed class SignatureHelpModelProviderService : ISignatureHelpModelProviderService
    {
        private readonly IReadOnlyCollection<ISignatureHelpModelProvider> _providers;

        [ImportingConstructor]
        public SignatureHelpModelProviderService([ImportMany] IEnumerable<ISignatureHelpModelProvider> providers)
        {
            _providers = providers.Concat(SignatureHelpExtensions.GetStandardSignatureHelpModelProviders()).ToArray();
        }

        public IReadOnlyCollection<ISignatureHelpModelProvider> Providers
        {
            get { return _providers; }
        }
    }
}