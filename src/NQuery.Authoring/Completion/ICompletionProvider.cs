namespace NQuery.Authoring.Completion;

public interface ICompletionProvider
{
    IEnumerable<CompletionItem> GetItems(DocumentView view, CancellationToken cancellationToken);
}
