using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor.Completion
{
    public interface ICompletionModelManagerProvider
    {
        ICompletionModelManager GetCompletionModel(ITextView textView);
    }
}