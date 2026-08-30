namespace NQuery.Authoring.QuickInfo;

public interface IQuickInfoModelProvider
{
    QuickInfoModel? GetModel(DocumentView view, CancellationToken cancellationToken);
}
