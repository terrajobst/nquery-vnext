using System;
using System.Collections.Generic;

using NQuery.Authoring.Completion;

namespace NQuery.Authoring.Composition.Completion
{
    public interface ICompletionProviderService
    {
        IReadOnlyCollection<ICompletionProvider> Providers { get; }
    }
}