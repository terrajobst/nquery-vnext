using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    [Export(typeof(ISignatureHelpManagerProvider))]
    internal sealed class SignatureHelpManagerProvider : ISignatureHelpManagerProvider
    {
        [Import]
        public INQuerySemanticModelManagerService SemanticModelManagerService { get; set; }

        [Import]
        public ISignatureHelpBroker SignatureHelpBroker { get; set; }

        [ImportMany]
        public IEnumerable<ISignatureModelProvider> SignatureModelProviders { get; set; }

        public ISignatureHelpManager GetSignatureHelpManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var semanticModelManager = SemanticModelManagerService.GetSemanticModelManager(textView.TextBuffer);
                return new SignatureHelpManager(textView, semanticModelManager, SignatureHelpBroker, SignatureModelProviders);
            });
        }
    }
}