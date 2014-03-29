using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Composition.CodeActions
{
    [Export(typeof(ICodeIssueProviderService))]
    internal sealed class CodeIssueProviderService : ICodeIssueProviderService
    {
        private readonly ImmutableArray<ICodeIssueProvider> _providers;

        [ImportingConstructor]
        public CodeIssueProviderService([ImportMany] IEnumerable<ICodeIssueProvider> matchers)
        {
            _providers = matchers.Concat(CodeActionExtensions.GetStandardIssueProviders()).ToImmutableArray();
        }

        public ImmutableArray<ICodeIssueProvider> Providers
        {
            get { return _providers; }
        }
    }
}