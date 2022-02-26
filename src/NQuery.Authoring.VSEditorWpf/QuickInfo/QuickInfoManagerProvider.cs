using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Composition.QuickInfo;

namespace NQuery.Authoring.VSEditorWpf.QuickInfo
{
    [Export(typeof(IQuickInfoManagerProvider))]
    internal sealed class QuickInfoManagerProvider : IQuickInfoManagerProvider
    {
        [Import]
        public IQuickInfoBroker QuickInfoBroker { get; set; }

        [Import]
        public IQuickInfoModelProviderService QuickInfoModelProviderService { get; set; }

        public IQuickInfoManager GetQuickInfoManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var workspace = textView.TextBuffer.GetWorkspace();
                return new QuickInfoManager(workspace, textView, QuickInfoBroker, QuickInfoModelProviderService);
            });
        }
    }
}