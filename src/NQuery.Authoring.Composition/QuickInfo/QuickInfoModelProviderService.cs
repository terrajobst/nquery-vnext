using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.Composition.QuickInfo
{
    [Export(typeof(IQuickInfoModelProviderService))]
    internal sealed class QuickInfoModelProviderService : IQuickInfoModelProviderService
    {
        private readonly ImmutableArray<IQuickInfoModelProvider> _providers;

        [ImportingConstructor]
        public QuickInfoModelProviderService([ImportMany] IEnumerable<IQuickInfoModelProvider> providers)
        {
            _providers = providers.Concat(QuickInfoExtensions.GetStandardQuickInfoModelProviders()).ToImmutableArray();
        }

        public ImmutableArray<IQuickInfoModelProvider> Providers
        {
            get { return _providers; }
        }
    }
}