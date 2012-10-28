using System;
using System.ComponentModel.Composition;

namespace NQuery.Authoring.QuickInfo
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class PropertyAccessExpressionQuickInfoModelProvider : QuickInfoModelProvider<PropertyAccessExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, PropertyAccessExpressionSyntax node)
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