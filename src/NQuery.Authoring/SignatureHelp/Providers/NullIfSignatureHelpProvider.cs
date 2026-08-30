using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.SignatureHelp.Providers;

internal sealed class NullIfSignatureHelpProvider : SignatureHelpProvider<NullIfExpressionSyntax>
{
    protected override SignatureHelpResult? GetResult(SemanticModel semanticModel, NullIfExpressionSyntax node, int position)
    {
        var span = node.Span;
        var signature = SignatureHelpExtensions.GetNullIfSignatureItem();
        var signatures = new[] { signature };

        var commaToken = node.CommaToken;
        var isBeforeComma = commaToken.IsMissing || position <= commaToken.Span.Start;
        var parameterIndex = isBeforeComma ? 0 : 1;

        return new SignatureHelpResult(span, signatures, signature, parameterIndex);
    }
}
