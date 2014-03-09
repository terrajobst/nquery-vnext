using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Composition.Completion;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.Completion
{
    [Export(typeof(ICompletionModelManagerProvider))]
    internal sealed class CompletionModelManagerProvider : ICompletionModelManagerProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        [Import]
        public ICompletionBroker CompletionBroker { get; set; }

        [Import]
        public ICompletionProviderService CompletionProviderService { get; set; }

        public ICompletionModelManager GetCompletionModel(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var document = DocumentManager.GetDocument(textView.TextBuffer);
                return new CompletionModelManager(textView, document, CompletionBroker, CompletionProviderService);
            });
        }
    }
}