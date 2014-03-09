using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.Composition.QuickInfo
{
    [Export(typeof(IQuickInfoModelProviderService))]
    internal sealed class QuickInfoModelProviderService : IQuickInfoModelProviderService
    {
        private readonly IReadOnlyCollection<IQuickInfoModelProvider> _providers;

        [ImportingConstructor]
        public QuickInfoModelProviderService([ImportMany] IEnumerable<IQuickInfoModelProvider> providers)
        {
            _providers = providers.Concat(QuickInfoExtensions.GetStandardQuickInfoModelProviders()).ToArray();
        }

        public IReadOnlyCollection<IQuickInfoModelProvider> Providers
        {
            get { return _providers; }
        }
    }
}