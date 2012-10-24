using System;

namespace NQuery.Language.Services.SignatureHelp
{
    public interface ISignatureModelProvider
    {
        SignatureHelpModel GetModel(SemanticModel semanticModel, int position);
    }
}