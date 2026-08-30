namespace NQuery.Authoring.CodeActions;

public interface ICodeFixProvider
{
    IEnumerable<ICodeAction> GetFixes(DocumentView view, CancellationToken cancellationToken);
}
