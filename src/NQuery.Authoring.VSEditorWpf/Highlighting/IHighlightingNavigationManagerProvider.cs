using System;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.Highlighting
{
    public interface IHighlightingNavigationManagerProvider
    {
        IHighlightingNavigationManager GetHighlightingNavigationManager(ITextView textView);
    }
}