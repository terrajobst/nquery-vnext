using System;
using System.Collections.Immutable;

using NQuery.Authoring.Completion;

namespace NQuery.Authoring.Composition.Completion
{
    public interface ICompletionProviderService
    {
        ImmutableArray<ICompletionProvider> Providers { get; }
    }
}