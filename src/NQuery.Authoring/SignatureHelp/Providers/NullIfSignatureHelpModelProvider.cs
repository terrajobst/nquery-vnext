using System;

using NQuery.Syntax;

namespace NQuery.Authoring.SignatureHelp.Providers
{
    internal sealed class NullIfSignatureHelpModelProvider : SignatureHelpModelProvider<NullIfExpressionSyntax>
    {
        protected override SignatureHelpModel GetModel(SemanticModel semanticModel, NullIfExpressionSyntax node, int position)
        {
            var span = node.Span;
            var signature = SignatureHelpExtensions.GetNullIfSignatureItem();
            var signatures = new[] { signature };

            var commaToken = node.CommaToken;
            var isBeforeComma = commaToken.IsMissing || position <= commaToken.Span.Start;
            var parameterIndex = isBeforeComma ? 0 : 1;

            return new SignatureHelpModel(span, signatures, signature, parameterIndex);
        }
    }
}