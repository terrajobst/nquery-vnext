namespace NQuery.Authoring.QuickInfo;

public interface IQuickInfoProvider
{
    QuickInfoResult? GetResult(DocumentView view, CancellationToken cancellationToken);
}
