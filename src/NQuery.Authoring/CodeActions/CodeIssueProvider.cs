using NQuery.CodeAnalysis;

namespace NQuery.Authoring.CodeActions;

public abstract class CodeIssueProvider<T> : ICodeIssueProvider
    where T : SyntaxNode
{
    public IEnumerable<CodeIssue> GetIssues(Document document, CancellationToken cancellationToken)
    {
        ThrowIfNull(document);

        var semanticModel = document.GetSemanticModel(cancellationToken);
        var nodes = semanticModel.SyntaxTree.Root.DescendantNodes().OfType<T>();
        return nodes.SelectMany(node => GetIssues(semanticModel, node));
    }

    protected abstract IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, T node);
}
