using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.Selection
{
    [Export(typeof(INQuerySelectionProviderService))]
    internal sealed class NQuerySelectionProviderService : INQuerySelectionProviderService
    {
        public INQuerySelectionProvider GetSelectionProvider(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                return new NQuerySelectionProvider(textView);
            });
        }
    }
}