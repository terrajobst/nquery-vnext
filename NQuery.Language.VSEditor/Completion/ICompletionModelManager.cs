using System;

using NQuery.Language.Services.Completion;

namespace NQuery.Language.VSEditor.Completion
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