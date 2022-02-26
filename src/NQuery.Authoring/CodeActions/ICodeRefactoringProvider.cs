namespace NQuery.Authoring.CodeActions
{
    public interface ICodeRefactoringProvider
    {
        IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position);
    }
}