using System;
using System.Text;

using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using SignatureItem = NQuery.Authoring.SignatureHelp.SignatureItem;

namespace NQuery.Authoring.ActiproWpf.SignatureHelp
{
    // TODO: I think we should use the SymbolMarkup here to ensure the signature has a pretty look & feel.
    internal sealed class NQuerySignatureContentProvider : IContentProvider
    {
        private readonly SignatureItem _signatureItem;
        private readonly int _parameterIndex;
        private readonly Lazy<IContentProvider> _lazyContentProvider;

        public NQuerySignatureContentProvider(SignatureItem signatureItem, int parameterIndex)
        {
            _signatureItem = signatureItem;
            _parameterIndex = parameterIndex;
            _lazyContentProvider = new Lazy<IContentProvider>(GetHtmlContent);
        }

        private IContentProvider GetHtmlContent()
        {
            var parameter = _parameterIndex < _signatureItem.Parameters.Length
                                ? _signatureItem.Parameters[_parameterIndex]
                                : null;

            var signatureText = _signatureItem.Content;
            if (parameter == null)
                return new DirectContentProvider(signatureText);

            var start = parameter.Span.Start;
            var length = parameter.Span.Length;
            var end = parameter.Span.End;

            var beforeParameterText = signatureText.Substring(0, start);
            var parameterText = signatureText.Substring(start, length);
            var postParameterText = signatureText.Substring(end);

            var sb = new StringBuilder();
            sb.Append(beforeParameterText);
            sb.Append(@"<b>");
            sb.Append(HtmlContentProvider.Escape(parameterText));
            sb.Append(@"</b>");
            sb.Append(postParameterText);
            var htmlSnippet = sb.ToString();
            return new HtmlContentProvider(htmlSnippet);
        }

        public object GetContent()
        {
            return _lazyContentProvider.Value.GetContent();
        }
    }
}