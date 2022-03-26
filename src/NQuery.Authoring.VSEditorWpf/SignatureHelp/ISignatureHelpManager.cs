using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.VSEditorWpf.SignatureHelp
{
    public interface ISignatureHelpManager
    {
        Task HandleTextInputAsync(string text);
        void HandlePreviewTextInput(string text);
        Task TriggerSignatureHelpAsync();

        SignatureHelpModel Model { get; }
        event EventHandler<EventArgs> ModelChanged;
    }
}