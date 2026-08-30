using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class MethodInvocationExpressionQuickInfoProvider : QuickInfoProvider<MethodInvocationExpressionSyntax>
{
    protected override QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, MethodInvocationExpressionSyntax node)
    {
        if (!node.IdentifierToken.Span.ContainsOrTouches(position))
            return null;

        var symbol = semanticModel.GetSymbol(node);
        return symbol is null
                   ? null
                   : QuickInfoResult.ForSymbol(semanticModel, node.IdentifierToken.Span, symbol);
    }
}
