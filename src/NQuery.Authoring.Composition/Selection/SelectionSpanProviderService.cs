using System.Collections.Immutable;
using System.ComponentModel.Composition;

using NQuery.Authoring.Selection;

namespace NQuery.Authoring.Composition.Selection;

[Export(typeof(ISelectionSpanProviderService))]
internal sealed class SelectionSpanProviderService : ISelectionSpanProviderService
{
    [ImportingConstructor]
    public SelectionSpanProviderService([ImportMany] IEnumerable<ISelectionSpanProvider> providers)
    {
        ThrowIfNull(providers);

        Providers = SelectionExtensions.StandardSelectionSpanProviders.AddRange(providers);
    }

    public ImmutableArray<ISelectionSpanProvider> Providers { get; }
}
