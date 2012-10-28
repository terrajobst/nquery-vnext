using System;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.QuickInfo
{
    public interface IQuickInfoManagerProvider
    {
        IQuickInfoManager GetQuickInfoManager(ITextView textView);
    }
}