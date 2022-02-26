using System.Collections.Immutable;

using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.Composition.QuickInfo
{
    public interface IQuickInfoModelProviderService
    {
        ImmutableArray<IQuickInfoModelProvider> Providers { get; }
    }
}