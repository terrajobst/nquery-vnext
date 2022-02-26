using System.Collections.Immutable;
using System.ComponentModel.Composition;

using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.Composition.QuickInfo
{
    [Export(typeof(IQuickInfoModelProviderService))]
    internal sealed class QuickInfoModelProviderService : IQuickInfoModelProviderService
    {
        [ImportingConstructor]
        public QuickInfoModelProviderService([ImportMany] IEnumerable<IQuickInfoModelProvider> providers)
        {
            Providers = providers.Concat(QuickInfoExtensions.GetStandardQuickInfoModelProviders()).ToImmutableArray();
        }

        public ImmutableArray<IQuickInfoModelProvider> Providers { get; }
    }
}