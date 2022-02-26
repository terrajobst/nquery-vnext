using NQuery.Syntax;

namespace NQuery.Authoring.SignatureHelp.Providers
{
    internal sealed class CastSignatureHelpModelProvider : SignatureHelpModelProvider<CastExpressionSyntax>
    {
        protected override SignatureHelpModel GetModel(SemanticModel semanticModel, CastExpressionSyntax node, int position)
        {
            var span = node.Span;
            var signature = SignatureHelpExtensions.GetCastSignatureItem();
            var signatures = new[] { signature };

            var asKeyword = node.AsKeyword;
            var isBeforeAsKeyword = asKeyword.IsMissing || position <= asKeyword.Span.Start;
            var parameterIndex = isBeforeAsKeyword ? 0 : 1;

            return new SignatureHelpModel(span, signatures, signature, parameterIndex);
        }
    }
}