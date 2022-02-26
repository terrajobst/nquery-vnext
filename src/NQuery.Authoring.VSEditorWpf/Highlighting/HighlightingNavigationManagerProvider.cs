using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace NQuery.Authoring.VSEditorWpf.Highlighting
{
    [Export(typeof(IHighlightingNavigationManagerProvider))]
    internal sealed class HighlightingNavigationManagerProvider : IHighlightingNavigationManagerProvider
    {
        [Import]
        public IViewTagAggregatorFactoryService AggregatorFactoryService { get; set; }

        public IHighlightingNavigationManager GetHighlightingNavigationManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var tagAggregator = AggregatorFactoryService.CreateTagAggregator<HighlightTag>(textView);
                return new HighlightingNavigationManager(textView, tagAggregator);
            });
        }
    }
}