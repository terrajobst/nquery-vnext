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
        [ImportingConstructor]
        public CodeIssueProviderService([ImportMany] IEnumerable<ICodeIssueProvider> matchers)
        {
            Providers = matchers.Concat(CodeActionExtensions.GetStandardIssueProviders()).ToImmutableArray();
        }

        public ImmutableArray<ICodeIssueProvider> Providers { get; }
    }
}