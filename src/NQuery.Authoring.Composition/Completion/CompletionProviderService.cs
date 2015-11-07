using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.Completion;

namespace NQuery.Authoring.Composition.Completion
{
    [Export(typeof(ICompletionProviderService))]
    internal sealed class CompletionProviderService : ICompletionProviderService
    {
        [ImportingConstructor]
        public CompletionProviderService([ImportMany] IEnumerable<ICompletionProvider> providers)
        {
            Providers = providers.Concat(CompletionExtensions.GetStandardCompletionProviders()).ToImmutableArray();
        }

        public ImmutableArray<ICompletionProvider> Providers { get; }
    }
}