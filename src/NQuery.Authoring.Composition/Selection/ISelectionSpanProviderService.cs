using System.Collections.Immutable;
using NQuery.Authoring.Selection;

namespace NQuery.Authoring.Composition.Selection
{
    public interface ISelectionSpanProviderService
    {
        ImmutableArray<ISelectionSpanProvider> Providers { get; }
    }
}