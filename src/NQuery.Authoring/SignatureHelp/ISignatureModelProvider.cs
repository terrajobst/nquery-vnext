using System;

namespace NQuery.Authoring.SignatureHelp
{
    public interface ISignatureModelProvider
    {
        SignatureHelpModel GetModel(SemanticModel semanticModel, int position);
    }
}