using System.Collections.Immutable;
using System.ComponentModel.Composition;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Composition.CodeActions;

[Export(typeof(ICodeRefactoringProviderService))]
internal sealed class CodeRefactoringProviderService : ICodeRefactoringProviderService
{
    [ImportingConstructor]
    public CodeRefactoringProviderService([ImportMany] IEnumerable<ICodeRefactoringProvider> matchers)
    {
        ThrowIfNull(matchers);

        Providers = CodeActionExtensions.StandardRefactoringProviders.AddRange(matchers);
    }

    public ImmutableArray<ICodeRefactoringProvider> Providers { get; }
}
