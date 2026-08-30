using System.Collections.Immutable;

namespace NQuery.Authoring.CodeActions;

public sealed class CodeIssueService
{
    private readonly ImmutableArray<ICodeIssueProvider> _providers;

    public CodeIssueService(ImmutableArray<ICodeIssueProvider> providers)
    {
        _providers = providers;
    }

    public ImmutableArray<CodeIssue> GetIssues(Document document, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);

        var semanticModel = document.GetSemanticModel(cancellationToken);
        return [.. _providers.SelectMany(p => p.GetIssues(semanticModel))];
    }
}
