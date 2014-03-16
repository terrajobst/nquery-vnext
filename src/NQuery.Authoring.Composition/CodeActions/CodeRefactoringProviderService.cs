using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Composition.CodeActions
{
    [Export(typeof(ICodeRefactoringProviderService))]
    internal sealed class CodeRefactoringProviderService : ICodeRefactoringProviderService
    {
        private readonly IReadOnlyCollection<ICodeRefactoringProvider> _providers;

        [ImportingConstructor]
        public CodeRefactoringProviderService([ImportMany] IEnumerable<ICodeRefactoringProvider> matchers)
        {
            _providers = matchers.Concat(CodeActionExtensions.GetStandardRefactoringProviders()).ToArray();
        }

        public IReadOnlyCollection<ICodeRefactoringProvider> Providers
        {
            get { return _providers; }
        }
    }
}