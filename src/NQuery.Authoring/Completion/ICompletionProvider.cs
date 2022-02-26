namespace NQuery.Authoring.Completion
{
    public interface ICompletionProvider
    {
        IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position);
    }
}