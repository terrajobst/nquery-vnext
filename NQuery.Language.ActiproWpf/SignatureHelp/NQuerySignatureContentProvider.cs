using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using NQuery.Language.VSEditor.SignatureHelp;

namespace NQueryViewerActiproWpf.SignatureHelp
{
    internal sealed class NQuerySignatureContentProvider : IContentProvider
    {
        private readonly SignatureItem _signatureItem;

        public NQuerySignatureContentProvider(SignatureItem signatureItem)
        {
            _signatureItem = signatureItem;
        }

        public object GetContent()
        {
            return _signatureItem.Content;
        }
    }
}