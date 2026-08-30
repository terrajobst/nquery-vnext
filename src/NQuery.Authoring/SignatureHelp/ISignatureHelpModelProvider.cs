namespace NQuery.Authoring.SignatureHelp;

public interface ISignatureHelpModelProvider
{
    SignatureHelpModel? GetModel(DocumentView view, CancellationToken cancellationToken);
}
