using NQuery.Syntax;

namespace NQuery.Authoring.SignatureHelp.Providers
{
    internal sealed class CoalesceSignatureHelpModelProvider : SignatureHelpModelProvider<CoalesceExpressionSyntax>
    {
        protected override SignatureHelpModel GetModel(SemanticModel semanticModel, CoalesceExpressionSyntax node, int position)
        {
            var span = node.Span;
            var signature = SignatureHelpExtensions.GetCoalesceSignatureItem();
            var signatures = new[] { signature };
            var parameterIndex = node.ArgumentList.GetParameterIndex(position);

            return new SignatureHelpModel(span, signatures, signature, parameterIndex);
        }
    }
}