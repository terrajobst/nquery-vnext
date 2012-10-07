using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoManagerProvider))]
    internal sealed class QuickInfoManagerProvider : IQuickInfoManagerProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        [Import]
        public IQuickInfoBroker QuickInfoBroker { get; set; }

        public IQuickInfoManager GetQuickInfoManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var document = DocumentManager.GetDocument(textView.TextBuffer);
                return new QuickInfoManager(textView, document, QuickInfoBroker);
            });
        }
    }
}