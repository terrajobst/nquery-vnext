using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class FunctionInvocationExpressionQuickInfoProvider : QuickInfoProvider<FunctionInvocationExpressionSyntax>
{
    protected override QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, FunctionInvocationExpressionSyntax node)
    {
        if (!node.IdentifierToken.Span.ContainsOrTouches(position))
            return null;

        var symbol = semanticModel.GetSymbol(node);
        return symbol is null
                   ? null
                   : QuickInfoResult.ForSymbol(semanticModel, node.IdentifierToken.Span, symbol);
    }
}
