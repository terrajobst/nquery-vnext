using System;
using System.ComponentModel.Composition;

namespace NQuery.Language.Services.QuickInfo
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class MethodInvocationExpressionQuickInfoModelProvider : QuickInfoModelProvider<MethodInvocationExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, MethodInvocationExpressionSyntax node)
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