using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

using NQuery.Language.VSEditor.Completion;
using NQuery.Language.VSEditor.SignatureHelp;

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

        [Import]
        public ISignatureHelpManagerProvider SignatureHelpManagerProvider { get; set; }

        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var completionModelManager = CompletionModelManagerProvider.GetCompletionModel(wpfTextView);
                var signatureHelpManager = SignatureHelpManagerProvider.GetSignatureHelpManager(wpfTextView);
                return new NQueryKeyProcessor(wpfTextView, IntellisenseSessionStackMapService, completionModelManager, signatureHelpManager);
            });
        }
    }
}