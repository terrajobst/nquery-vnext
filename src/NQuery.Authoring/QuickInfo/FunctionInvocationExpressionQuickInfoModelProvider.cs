using System;
using System.ComponentModel.Composition;

using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class FunctionInvocationExpressionQuickInfoModelProvider : QuickInfoModelProvider<FunctionInvocationExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, FunctionInvocationExpressionSyntax node)
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