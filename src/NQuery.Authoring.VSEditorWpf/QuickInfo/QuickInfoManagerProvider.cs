using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Composition.QuickInfo;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.QuickInfo
{
    [Export(typeof(IQuickInfoManagerProvider))]
    internal sealed class QuickInfoManagerProvider : IQuickInfoManagerProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        [Import]
        public IQuickInfoBroker QuickInfoBroker { get; set; }

        [Import]
        public IQuickInfoModelProviderService QuickInfoModelProviderService { get; set; }

        public IQuickInfoManager GetQuickInfoManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var document = DocumentManager.GetDocument(textView.TextBuffer);
                return new QuickInfoManager(textView, document, QuickInfoBroker, QuickInfoModelProviderService);
            });
        }
    }
}