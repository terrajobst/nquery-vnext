using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.VSEditorWpf.CodeActions;
using NQuery.Authoring.VSEditorWpf.Commenting;
using NQuery.Authoring.VSEditorWpf.Completion;
using NQuery.Authoring.VSEditorWpf.Highlighting;
using NQuery.Authoring.VSEditorWpf.SignatureHelp;

namespace NQuery.Authoring.VSEditorWpf
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

        [Import]
        public IHighlightingNavigationManagerProvider HighlightingNavigationManagerProvider { get; set; }

        [Import]
        public ICodeActionGlyphBroker CodeActionGlyphBroker { get; set; }

        [Import]
        public ICommentOperationsProvider CommentOperationsProvider { get; set; }

        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var completionModelManager = CompletionModelManagerProvider.GetCompletionModel(wpfTextView);
                var signatureHelpManager = SignatureHelpManagerProvider.GetSignatureHelpManager(wpfTextView);
                var highlightingNavigationManager = HighlightingNavigationManagerProvider.GetHighlightingNavigationManager(wpfTextView);
                var commentOperations = CommentOperationsProvider.GetCommentOperations(wpfTextView);
                return new NQueryKeyProcessor(wpfTextView, IntellisenseSessionStackMapService, completionModelManager, signatureHelpManager, highlightingNavigationManager, CodeActionGlyphBroker, commentOperations);
            });
        }
    }
}