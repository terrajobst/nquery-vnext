using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Composition.SignatureHelp;

namespace NQuery.Authoring.VSEditorWpf.SignatureHelp;

[Export(typeof(ISignatureHelpManagerProvider))]
internal sealed class SignatureHelpManagerProvider : ISignatureHelpManagerProvider
{
    [Import]
    public ISignatureHelpBroker SignatureHelpBroker { get; set; } = null!;

    [Import]
    public ISignatureHelpModelProviderService SignatureHelpModelProviderService { get; set; } = null!;

    public ISignatureHelpManager GetSignatureHelpManager(ITextView textView)
    {
        return textView.Properties.GetOrCreateSingletonProperty(() =>
        {
            return new SignatureHelpManager(textView, SignatureHelpBroker, SignatureHelpModelProviderService);
        });
    }
}
