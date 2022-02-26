using System.Collections.Immutable;
using System.ComponentModel.Composition;

using NQuery.Authoring.Selection;

namespace NQuery.Authoring.Composition.Selection
{
    [Export(typeof(ISelectionSpanProviderService))]
    internal sealed class SelectionSpanProviderService : ISelectionSpanProviderService
    {
        [ImportingConstructor]
        public SelectionSpanProviderService([ImportMany] IEnumerable<ISelectionSpanProvider> providers)
        {
            Providers = providers.Concat(SelectionExtensions.GetStandardSelectionSpanProviders()).ToImmutableArray();
        }

        public ImmutableArray<ISelectionSpanProvider> Providers { get; }
    }
}
