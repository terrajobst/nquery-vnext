using System.Collections.ObjectModel;

using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.ActiproWpf.SignatureHelp
{
    public interface INQuerySignatureHelpProvider : IParameterInfoProvider
    {
        Collection<ISignatureHelpModelProvider> Providers { get; }
    }
}