using System;

using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class FunctionInvocationExpressionQuickInfoModelProvider : QuickInfoModelProvider<FunctionInvocationExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, FunctionInvocationExpressionSyntax node)
        {
            if (!node.Name.Span.ContainsOrTouches(position))
                return null;

            var symbol = semanticModel.GetSymbol(node);
            return symbol == null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, node.Name.Span, symbol);
        }
    }
}