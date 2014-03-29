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
        private readonly ImmutableArray<ICompletionProvider> _providers;

        [ImportingConstructor]
        public CompletionProviderService([ImportMany] IEnumerable<ICompletionProvider> providers)
        {
            _providers = providers.Concat(CompletionExtensions.GetStandardCompletionProviders()).ToImmutableArray();
        }

        public ImmutableArray<ICompletionProvider> Providers
        {
            get { return _providers; }
        }
    }
}