using System;

using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo
{
    internal sealed class VariableExpressionQuickInfoModelProvider : QuickInfoModelProvider<VariableExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, VariableExpressionSyntax node)
        {
            if (!node.Name.Span.Contains(position))
                return null;

            var symbol = semanticModel.GetSymbol(node);
            return symbol == null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, node.Name.Span, symbol);
        }
    }
}