using NQuery.Authoring.Completion;

namespace NQuery.Authoring.VSEditorWpf.Completion
{
    public interface ICompletionModelManager
    {
        void HandleTextInput(string text);
        void HandlePreviewTextInput(string text);
        void TriggerCompletion(bool autoComplete);
        bool Commit();

        CompletionModel Model { get; }
        event EventHandler<EventArgs> ModelChanged;
    }
}