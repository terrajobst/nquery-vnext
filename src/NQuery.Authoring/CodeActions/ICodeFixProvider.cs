namespace NQuery.Authoring.CodeActions
{
    public interface ICodeFixProvider
    {
        IEnumerable<ICodeAction> GetFixes(SemanticModel semanticModel, int position);
    }
}