using System;

namespace NQuery.Language.VSEditor.SignatureHelp
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