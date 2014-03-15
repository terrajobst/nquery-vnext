using System;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    public interface ICodeActionGlyphBroker
    {
        ICodeActionGlyphController GetController(ITextView textView);
    }
}