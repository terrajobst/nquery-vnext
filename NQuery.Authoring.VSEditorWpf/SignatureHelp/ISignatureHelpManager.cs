using System;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.VSEditorWpf.SignatureHelp
{
    public interface ISignatureHelpManager
    {
        void HandleTextInput(string text);
        void HandlePreviewTextInput(string text);
        void TriggerSignatureHelp();

        SignatureHelpModel Model { get; }
        event EventHandler<EventArgs> ModelChanged;
    }
}