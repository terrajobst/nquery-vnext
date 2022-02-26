using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.Selection
{
    public interface INQuerySelectionProviderService
    {
        INQuerySelectionProvider GetSelectionProvider(ITextView textView);
    }
}