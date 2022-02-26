using System.Collections.Immutable;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Composition.CodeActions
{
    public interface ICodeFixProviderService
    {
        ImmutableArray<ICodeFixProvider> Providers { get; }
    }
}