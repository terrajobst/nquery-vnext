using System;

namespace NQuery.Authoring.SignatureHelp
{
    public interface ISignatureHelpModelProvider
    {
        SignatureHelpModel GetModel(SemanticModel semanticModel, int position);
    }
}