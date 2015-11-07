using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Composition.CodeActions
{
    [Export(typeof(ICodeRefactoringProviderService))]
    internal sealed class CodeRefactoringProviderService : ICodeRefactoringProviderService
    {
        [ImportingConstructor]
        public CodeRefactoringProviderService([ImportMany] IEnumerable<ICodeRefactoringProvider> matchers)
        {
            Providers = matchers.Concat(CodeActionExtensions.GetStandardRefactoringProviders()).ToImmutableArray();
        }

        public ImmutableArray<ICodeRefactoringProvider> Providers { get; }
    }
}