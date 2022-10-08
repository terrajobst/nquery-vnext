using NQuery.Authoring.Completion;

namespace NQuery.Authoring.VSEditorWpf.Completion
{
    public interface ICompletionModelManager
    {
        Task HandleTextInputAsync(string text);
        void HandlePreviewTextInput(string text);
        Task TriggerCompletionAsync(bool autoComplete);
        bool Commit();

        CompletionModel Model { get; }
        event EventHandler<EventArgs> ModelChanged;
    }
}