using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor
{
    public interface IQuickInfoManagerProvider
    {
        IQuickInfoManager GetQuickInfoManager(ITextView textView);
    }
}