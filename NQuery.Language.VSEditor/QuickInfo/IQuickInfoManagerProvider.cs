using System;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor.QuickInfo
{
    public interface IQuickInfoManagerProvider
    {
        IQuickInfoManager GetQuickInfoManager(ITextView textView);
    }
}