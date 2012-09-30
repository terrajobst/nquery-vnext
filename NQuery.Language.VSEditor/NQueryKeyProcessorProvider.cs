using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using NQuery.Language.VSEditor.Completion;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IKeyProcessorProvider))]
    [Name("NQueryKeyProcessorProvider")]
    [ContentType("NQuery")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class NQueryKeyProcessorProvider : IKeyProcessorProvider
    {
        [Import]
        public IIntellisenseSessionStackMapService IntellisenseSessionStackMapService { get; set; }

        [Import]
        public ICompletionModelManagerProvider CompletionModelManagerProvider { get; set; }

        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var completionModel = CompletionModelManagerProvider.GetCompletionModel(wpfTextView);
                return new NQueryKeyProcessor(wpfTextView, IntellisenseSessionStackMapService, completionModel);
            });
        }
    }
}