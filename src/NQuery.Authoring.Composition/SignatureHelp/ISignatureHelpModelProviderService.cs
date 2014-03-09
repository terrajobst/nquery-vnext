using System;
using System.Collections.Generic;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.Composition.SignatureHelp
{
    public interface ISignatureHelpModelProviderService
    {
        IReadOnlyCollection<ISignatureHelpModelProvider> Providers { get; }
    }
}