using System;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.Rearrangement
{
    public interface IRearrangeModelManagerProvider
    {
        IRearrangeModelManager GetRearrangeModelManager(ITextView textView);
    }
}