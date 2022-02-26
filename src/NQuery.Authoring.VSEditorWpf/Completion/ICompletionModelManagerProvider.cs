using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.Completion
{
    public interface ICompletionModelManagerProvider
    {
        ICompletionModelManager GetCompletionModel(ITextView textView);
    }
}