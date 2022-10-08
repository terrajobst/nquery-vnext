using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.QuickInfo
{
    internal sealed class NQueryQuickInfoTrigger
    {
        private readonly IWpfTextView _wpfTextView;
        private readonly IQuickInfoManager _quickInfoManager;

        public NQueryQuickInfoTrigger(IWpfTextView wpfTextView, IQuickInfoManager quickInfoManager)
        {
            _wpfTextView = wpfTextView;
            _wpfTextView.MouseHover += WpfTextViewOnMouseHover;
            _quickInfoManager = quickInfoManager;
        }

        private void WpfTextViewOnMouseHover(object sender, MouseHoverEventArgs e)
        {
            _quickInfoManager.TriggerQuickInfoAsync(e.Position);
        }
    }
}