namespace NQuery.Language.VSEditor.Completion
{
    public interface ICompletionModelProvider
    {
        CompletionModel GetModel(SemanticModel semanticModel, int position);
    }
}