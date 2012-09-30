using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor.Completion
{
    [Export(typeof(ICompletionModelManagerProvider))]
    internal sealed class CompletionModelManagerProvider : ICompletionModelManagerProvider
    {
        [Import]
        public INQuerySemanticModelManagerService SemanticModelManagerService { get; set; }

        [Import]
        public ICompletionBroker CompletionBroker { get; set; }

        [ImportMany]
        public IEnumerable<ICompletionProvider> CompletionItemProviders { get; set; }

        public ICompletionModelManager GetCompletionModel(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var semanticModelManager = SemanticModelManagerService.GetSemanticModelManager(textView.TextBuffer);
                return new CompletionModelManager(textView, semanticModelManager, CompletionBroker, CompletionItemProviders);
            });
        }
    }
}