using System.Collections.Immutable;
using System.ComponentModel.Composition;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Composition.CodeActions
{
    [Export(typeof(ICodeFixProviderService))]
    internal sealed class CodeFixProviderService : ICodeFixProviderService
    {
        [ImportingConstructor]
        public CodeFixProviderService([ImportMany] IEnumerable<ICodeFixProvider> matchers)
        {
            Providers = matchers.Concat(CodeActionExtensions.GetStandardFixProviders()).ToImmutableArray();
        }

        public ImmutableArray<ICodeFixProvider> Providers { get; }
    }
}