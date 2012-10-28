using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.SignatureHelp
{
    [Export(typeof(ISignatureHelpManagerProvider))]
    internal sealed class SignatureHelpManagerProvider : ISignatureHelpManagerProvider
    {
        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        [Import]
        public ISignatureHelpBroker SignatureHelpBroker { get; set; }

        [ImportMany]
        public IEnumerable<ISignatureModelProvider> SignatureModelProviders { get; set; }

        public ISignatureHelpManager GetSignatureHelpManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var document = DocumentManager.GetDocument(textView.TextBuffer);
                return new SignatureHelpManager(textView, document, SignatureHelpBroker, SignatureModelProviders);
            });
        }
    }
}