using System;

using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class CommonTableExpressionQuickInfoModelProvider : QuickInfoModelProvider<CommonTableExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, CommonTableExpressionSyntax node)
        {
            if (!node.Name.Span.ContainsOrTouches(position))
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(node);
            return symbol == null
                ? null
                : QuickInfoModel.ForSymbol(semanticModel, node.Name.Span, symbol);
        }
    }
}