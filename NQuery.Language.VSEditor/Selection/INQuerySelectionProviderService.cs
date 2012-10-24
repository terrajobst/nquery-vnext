using System;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor.Selection
{
    public interface INQuerySelectionProviderService
    {
        INQuerySelectionProvider GetSelectionProvider(ITextView textView);
    }
}