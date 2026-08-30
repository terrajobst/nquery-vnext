namespace NQuery.Authoring.CodeActions;

public interface ICodeRefactoringProvider
{
    IEnumerable<ICodeAction> GetRefactorings(DocumentView view, CancellationToken cancellationToken);
}
