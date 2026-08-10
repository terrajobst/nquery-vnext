using System.Collections.Immutable;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Tests.CodeActions;

public abstract class CodeIssueTests
{
    protected ImmutableArray<CodeIssue> GetIssues(string query)
    {
        var services = DocumentFactory.ServicesWithOnly(CreateProvider());
        var document = DocumentFactory.CreateQuery(query, services);

        return document.Services.GetService<CodeIssueService>().GetIssues(document);
    }

    protected abstract ICodeIssueProvider CreateProvider();
}
