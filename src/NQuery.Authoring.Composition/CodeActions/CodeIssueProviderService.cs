using System.Collections.Immutable;
using System.ComponentModel.Composition;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Composition.CodeActions;

[Export(typeof(ICodeIssueProviderService))]
internal sealed class CodeIssueProviderService : ICodeIssueProviderService
{
    [ImportingConstructor]
    public CodeIssueProviderService([ImportMany] IEnumerable<ICodeIssueProvider> matchers)
    {
        ThrowIfNull(matchers);

        Providers = CodeActionExtensions.StandardIssueProviders.AddRange(matchers);
    }

    public ImmutableArray<ICodeIssueProvider> Providers { get; }
}
