namespace NQuery.Authoring.CodeActions;

public interface ICodeIssueProvider
{
    IEnumerable<CodeIssue> GetIssues(Document document, CancellationToken cancellationToken);
}
