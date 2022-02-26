using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Composition.Completion;

namespace NQuery.Authoring.VSEditorWpf.Completion
{
    [Export(typeof(ICompletionModelManagerProvider))]
    internal sealed class CompletionModelManagerProvider : ICompletionModelManagerProvider
    {
        [Import]
        public ICompletionBroker CompletionBroker { get; set; }

        [Import]
        public ICompletionProviderService CompletionProviderService { get; set; }

        public ICompletionModelManager GetCompletionModel(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var workspace = textView.TextBuffer.GetWorkspace();
                return new CompletionModelManager(workspace, textView, CompletionBroker, CompletionProviderService);
            });
        }
    }
}