using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.Selection
{
    [Export(typeof(INQuerySelectionProviderService))]
    internal sealed class NQuerySelectionProviderService : INQuerySelectionProviderService
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        public INQuerySelectionProvider GetSelectionProvider(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var document = DocumentManager.GetDocument(textView.TextBuffer);
                return new NQuerySelectionProvider(textView, document);
            });
        }
    }
}