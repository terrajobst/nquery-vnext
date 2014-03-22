using System;

using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class VariableExpressionQuickInfoModelProvider : QuickInfoModelProvider<VariableExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, VariableExpressionSyntax node)
        {
            var symbol = semanticModel.GetSymbol(node);
            return symbol == null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, node.Span, symbol);
        }
    }
}