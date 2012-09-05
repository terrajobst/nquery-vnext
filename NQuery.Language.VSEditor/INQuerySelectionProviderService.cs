using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor
{
    public interface INQuerySelectionProviderService
    {
        INQuerySelectionProvider GetSelectionProvider(ITextView textView);
    }
}