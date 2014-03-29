using System;
using System.Collections.Immutable;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.Composition.SignatureHelp
{
    public interface ISignatureHelpModelProviderService
    {
        ImmutableArray<ISignatureHelpModelProvider> Providers { get; }
    }
}