using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.SignatureHelp.Providers;

internal sealed class CoalesceSignatureHelpProvider : SignatureHelpProvider<CoalesceExpressionSyntax>
{
    protected override SignatureHelpResult? GetResult(SemanticModel semanticModel, CoalesceExpressionSyntax node, int position)
    {
        var span = node.Span;
        var signature = SignatureHelpExtensions.GetCoalesceSignatureItem();
        var signatures = new[] { signature };
        var parameterIndex = node.ArgumentList.GetParameterIndex(position);

        return new SignatureHelpResult(span, signatures, signature, parameterIndex);
    }
}
