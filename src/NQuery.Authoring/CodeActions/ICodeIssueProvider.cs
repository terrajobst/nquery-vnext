namespace NQuery.Authoring.CodeActions
{
    public interface ICodeIssueProvider
    {
        IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel);
    }
}