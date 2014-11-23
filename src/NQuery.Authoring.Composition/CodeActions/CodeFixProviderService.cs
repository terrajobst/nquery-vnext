using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Composition.CodeActions
{
    [Export(typeof(ICodeFixProviderService))]
    internal sealed class CodeFixProviderService : ICodeFixProviderService
    {
        private readonly ImmutableArray<ICodeFixProvider> _providers;

        [ImportingConstructor]
        public CodeFixProviderService([ImportMany] IEnumerable<ICodeFixProvider> matchers)
        {
            _providers = matchers.Concat(CodeActionExtensions.GetStandardFixProviders()).ToImmutableArray();
        }

        public ImmutableArray<ICodeFixProvider> Providers
        {
            get { return _providers; }
        }
    }
}