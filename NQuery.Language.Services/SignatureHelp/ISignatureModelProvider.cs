namespace NQuery.Language.VSEditor.SignatureHelp
{
    public interface ISignatureModelProvider
    {
        SignatureHelpModel GetModel(SemanticModel semanticModel, int position);
    }
}