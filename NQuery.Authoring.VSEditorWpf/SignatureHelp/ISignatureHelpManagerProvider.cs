using System;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.SignatureHelp
{
    public interface ISignatureHelpManagerProvider
    {
        ISignatureHelpManager GetSignatureHelpManager(ITextView textView);
    }
}