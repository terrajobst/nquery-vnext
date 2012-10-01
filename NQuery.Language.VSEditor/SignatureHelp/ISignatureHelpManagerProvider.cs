using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    public interface ISignatureHelpManagerProvider
    {
        ISignatureHelpManager GetSignatureHelpManager(ITextView textView);
    }
}