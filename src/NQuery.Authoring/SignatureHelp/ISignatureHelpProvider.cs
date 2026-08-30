namespace NQuery.Authoring.SignatureHelp;

public interface ISignatureHelpProvider
{
    SignatureHelpResult? GetResult(DocumentView view, CancellationToken cancellationToken);
}
