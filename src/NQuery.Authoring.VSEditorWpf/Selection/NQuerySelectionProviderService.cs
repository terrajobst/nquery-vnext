using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;
using NQuery.Authoring.Composition.Selection;

namespace NQuery.Authoring.VSEditorWpf.Selection
{
    [Export(typeof(INQuerySelectionProviderService))]
    internal sealed class NQuerySelectionProviderService : INQuerySelectionProviderService
    {
        [Import]
        public ISelectionSpanProviderService SelectionSpanProviderService { get; set; }

        public INQuerySelectionProvider GetSelectionProvider(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                return new NQuerySelectionProvider(textView, SelectionSpanProviderService);
            });
        }
    }
}