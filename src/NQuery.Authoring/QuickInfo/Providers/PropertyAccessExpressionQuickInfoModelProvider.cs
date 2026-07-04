using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class PropertyAccessExpressionQuickInfoModelProvider : QuickInfoModelProvider<PropertyAccessExpressionSyntax>
{
    protected override QuickInfoModel? CreateModel(SemanticModel semanticModel, int position, PropertyAccessExpressionSyntax node)
    {
        if (!node.IdentifierToken.Span.ContainsOrTouches(position))
            return null;

        var symbol = semanticModel.GetSymbol(node);
        return symbol is null
                   ? null
                   : QuickInfoModel.ForSymbol(semanticModel, node.IdentifierToken.Span, symbol);
    }
}
